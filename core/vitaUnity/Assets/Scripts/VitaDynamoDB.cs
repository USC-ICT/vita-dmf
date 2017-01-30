using Amazon;
using Amazon.Runtime;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.DataModel;
using Amazon.CognitoIdentity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class VitaDynamoDB : MonoBehaviour
{
    public delegate void CallbackDelegate(object par);
    public delegate void CallbackResultDelegate(object par, string exception);
    public delegate void CallbackDelegate<T>(List<T> par);
    public delegate IEnumerator CallbackDelegateCoroutine(object par);

    public const string TableNameEntityDBVitaProfile = "VitaProfile";

    public class CognitoIdentity
    {
        public string IdentityPoolId;
        public string CognitoPoolRegion;
        public string DynamoRegion;
    }

    public static CognitoIdentity IdentityPool_ICT = new CognitoIdentity() { IdentityPoolId = "us-xxxx-x:xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx", CognitoPoolRegion = RegionEndpoint.USEast1.SystemName, DynamoRegion = RegionEndpoint.USEast1.SystemName };
    public static CognitoIdentity IdentityPool_ED = new CognitoIdentity()  { IdentityPoolId = "us-xxxx-x:xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx", CognitoPoolRegion = RegionEndpoint.USWest2.SystemName, DynamoRegion = RegionEndpoint.USWest2.SystemName };
    public static CognitoIdentity IdentityPool_DMF = new CognitoIdentity() { IdentityPoolId = "us-xxxx-x:xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx", CognitoPoolRegion = RegionEndpoint.USEast1.SystemName, DynamoRegion = RegionEndpoint.USEast1.SystemName };

    public string IdentityPoolId = IdentityPool_ICT.IdentityPoolId;
    public string CognitoPoolRegion = IdentityPool_ICT.CognitoPoolRegion;
    public string DynamoRegion = IdentityPool_ICT.DynamoRegion;

    static IAmazonDynamoDB _ddbClient;
    AWSCredentials _credentials;
    DynamoDBContext _context;


    RegionEndpoint _CognitoPoolRegion
    {
        get { return RegionEndpoint.GetBySystemName(CognitoPoolRegion); }
    }

    RegionEndpoint _DynamoRegion
    {
        get { return RegionEndpoint.GetBySystemName(DynamoRegion); }
    }

    AWSCredentials Credentials
    {
        get
        {
            if (_credentials == null)
                _credentials = new CognitoAWSCredentials(IdentityPoolId, _CognitoPoolRegion);

            return _credentials;
        }
    }

    IAmazonDynamoDB Client
    {
        get
        {
            if (_ddbClient == null)
                _ddbClient = new AmazonDynamoDBClient(Credentials, _DynamoRegion);

            return _ddbClient;
        }
    }

    DynamoDBContext Context
    {
        get
        {
            if (_context == null)
                _context = new DynamoDBContext(Client);

            return _context;
        }
    }



    public void ClearIdentityData()
    {
           _credentials = null;
           _ddbClient   = null;
           _context     = null;
    }

    void Awake()
    {
        if (VHUtils.IsWebGL())
            return;


        UnityInitializer.AttachToGameObject(this.gameObject);

        var loggingConfig = AWSConfigs.LoggingConfig;
        loggingConfig.LogTo = LoggingOptions.UnityLogger;
        loggingConfig.LogMetrics = false;
        loggingConfig.LogResponses = ResponseLoggingOption.OnError;
        loggingConfig.LogResponsesSizeLimit = 4096;
        loggingConfig.LogMetricsFormat = LogMetricsFormatOption.JSON;

    }

    void Start()
    {
    }


    void CreateTableVitaOrganization()
    {
        string tableName = "VitaOrganization";

        var VitaOrganization = new CreateTableRequest
        {
            AttributeDefinitions = new List<AttributeDefinition>()
            {
                new AttributeDefinition
                {
                    AttributeName = "name",
                    AttributeType = "S"
                }
            },

            KeySchema = new List<KeySchemaElement>
            {
                new KeySchemaElement
                {
                    AttributeName = "name",
                    KeyType = "HASH"
                }
            },

            ProvisionedThroughput = new ProvisionedThroughput
            {
                ReadCapacityUnits =2,
                WriteCapacityUnits = 2,
            },
            TableName = tableName
        };

        Client.CreateTableAsync(VitaOrganization, (result) =>
        {
            if (result.Exception != null)
            {
                Debug.Log(result.Exception.Message);
                return;
            }

            var tableDescription = result.Response.TableDescription;
            Debug.LogFormat("Created {0}: {1} - ReadsPerSec: {2} - WritesPerSec: {3}\nAllow a few seconds for changes to reflect...",
                tableDescription.TableName,
                tableDescription.TableStatus,
                tableDescription.ProvisionedThroughput.ReadCapacityUnits,
                tableDescription.ProvisionedThroughput.WriteCapacityUnits);
        });
    }

    void CreateTableVitaProfile()
    {
        string tableName = "VitaProfile";

        var ptIndex = new ProvisionedThroughput
        {
            ReadCapacityUnits = 1L,
            WriteCapacityUnits = 1L
        };

        var createIndex = new GlobalSecondaryIndex()
        {
            IndexName = "organization-index",
            ProvisionedThroughput = ptIndex,
            KeySchema = { new KeySchemaElement { AttributeName = "organization", KeyType = "HASH" } },
            Projection = new Projection { ProjectionType = "ALL" }
        };

        var createUsernameIndex = new GlobalSecondaryIndex()
        {
            IndexName = "username-index",
            ProvisionedThroughput = ptIndex,
            KeySchema = { new KeySchemaElement { AttributeName = "username", KeyType = "HASH" } },
            Projection = new Projection { ProjectionType = "ALL" }
        };

        var Profile = new CreateTableRequest
        {
            AttributeDefinitions = new List<AttributeDefinition>()
            {
                new AttributeDefinition
                {
                    AttributeName = "username",
                    AttributeType = "S"
                },
                new AttributeDefinition
                {
                    AttributeName = "password",
                    AttributeType = "S"
                },
                new AttributeDefinition
                {
                    AttributeName = "organization",
                    AttributeType = "S"
                },
            },

            KeySchema = new List<KeySchemaElement>
            {
                new KeySchemaElement
                {
                    AttributeName = "username",
                    KeyType = "HASH"
                },
                new KeySchemaElement
                {
                    AttributeName = "password",
                    KeyType = "RANGE"
                }
            },

            ProvisionedThroughput = new ProvisionedThroughput
            {
                    ReadCapacityUnits = 2,
                    WriteCapacityUnits = 2,
            },

            TableName = tableName,

            GlobalSecondaryIndexes = {createIndex, createUsernameIndex}
        };

        Client.CreateTableAsync(Profile, (result) =>
        {
            if (result.Exception != null)
            {
                Debug.Log(result.Exception.Message);
                return;
            }

            var tableDescription = result.Response.TableDescription;
            Debug.LogFormat("Created {0}: {1} - ReadsPerSec: {2} - WritesPerSec: {3}\nAllow a few seconds for changes to reflect...",
                tableDescription.TableName,
                tableDescription.TableStatus,
                tableDescription.ProvisionedThroughput.ReadCapacityUnits,
                tableDescription.ProvisionedThroughput.WriteCapacityUnits);
        });
    }

    public void CreateTableVitaClass()
    {
        string tableName = "VitaClass";

        var ptIndex = new ProvisionedThroughput
        {
            ReadCapacityUnits = 1L,
            WriteCapacityUnits = 1L
        };

        var createIndex = new GlobalSecondaryIndex()
        {
            IndexName               = "organization-index",
            ProvisionedThroughput   = ptIndex,
            KeySchema               = { new KeySchemaElement { AttributeName = "organization", KeyType = "HASH" } },
            Projection              = new Projection { ProjectionType = "ALL" }
        };

        var createIndexClassname = new GlobalSecondaryIndex()
        {
            IndexName               = "classname-index",
            ProvisionedThroughput   = ptIndex,
            KeySchema               = { new KeySchemaElement { AttributeName = "classname", KeyType = "HASH" } },
            Projection              = new Projection { ProjectionType = "ALL" }
        };

        var Tbl = new CreateTableRequest
        {
            AttributeDefinitions = new List<AttributeDefinition>()
            {
                new AttributeDefinition
                {
                    AttributeName = "classname",
                    AttributeType = "S"
                },
                new AttributeDefinition
                {
                    AttributeName = "organization",
                    AttributeType = "S"
                },
            },

            KeySchema = new List<KeySchemaElement>
            {
                new KeySchemaElement
                {
                    AttributeName = "classname",
                    KeyType = "HASH"
                }
            },

            ProvisionedThroughput = new ProvisionedThroughput
            {
                ReadCapacityUnits = 2,
                WriteCapacityUnits = 2
            },

            TableName = tableName,
            GlobalSecondaryIndexes = { createIndex, createIndexClassname }
        };

        Client.CreateTableAsync(Tbl, (result) =>
        {
            if (result.Exception != null)
            {
                Debug.Log(result.Exception.Message);
                return;
            }

            var tableDescription = result.Response.TableDescription;
            Debug.LogFormat("Created {0}: {1} - ReadsPerSec: {2} - WritesPerSec: {3}\nAllow a few seconds for changes to reflect...",
                tableDescription.TableName,
                tableDescription.TableStatus,
                tableDescription.ProvisionedThroughput.ReadCapacityUnits,
                tableDescription.ProvisionedThroughput.WriteCapacityUnits);
        });
    }

    public void CreateTableVitaStudentSession()
    {
        string tableName = "VitaStudentSession";

        var ptIndex = new ProvisionedThroughput
        {
            ReadCapacityUnits = 1L,
            WriteCapacityUnits = 1L
        };

        var createIndex = new GlobalSecondaryIndex()
        {
            IndexName = "organization-index",
            ProvisionedThroughput = ptIndex,
            KeySchema = { new KeySchemaElement { AttributeName = "organization", KeyType = "HASH" } },
            Projection = new Projection { ProjectionType = "ALL" }
        };

        var createUsernameIndex = new GlobalSecondaryIndex()
        {
            IndexName = "username-index",
            ProvisionedThroughput = ptIndex,
            KeySchema = { new KeySchemaElement { AttributeName = "username", KeyType = "HASH" } },
            Projection = new Projection { ProjectionType = "ALL" }
        };

        var Tbl = new CreateTableRequest
        {
            AttributeDefinitions = new List<AttributeDefinition>()
            {
                new AttributeDefinition
                {
                    AttributeName = "username",
                    AttributeType = "S"
                },
                new AttributeDefinition
                {
                    AttributeName = "sessionname",
                    AttributeType = "S"
                },
                new AttributeDefinition
                {
                    AttributeName = "organization",
                    AttributeType = "S"
                },
            },

            KeySchema = new List<KeySchemaElement>
            {
                new KeySchemaElement
                {
                    AttributeName = "username",
                    KeyType = "HASH"
                },
                new KeySchemaElement
                {
                    AttributeName = "sessionname",
                    KeyType = "RANGE"
                }
            },

            ProvisionedThroughput = new ProvisionedThroughput
            {
                ReadCapacityUnits =3,
                WriteCapacityUnits = 3,
            },

            TableName = tableName,

            GlobalSecondaryIndexes = {createIndex, createUsernameIndex}
        };

        Client.CreateTableAsync(Tbl, (result) =>
        {
            if (result.Exception != null)
            {
                Debug.Log(result.Exception.Message);
                return;
            }

            var tableDescription = result.Response.TableDescription;
            Debug.LogFormat("Created {0}: {1} - ReadsPerSec: {2} - WritesPerSec: {3}\nAllow a few seconds for changes to reflect...",
                tableDescription.TableName,
                tableDescription.TableStatus,
                tableDescription.ProvisionedThroughput.ReadCapacityUnits,
                tableDescription.ProvisionedThroughput.WriteCapacityUnits);
        });
    }

    public void CreateTableClassHomeworkAssignment()
    {
        string tableName = "VitaClassHomeworkAssigment";

        var ptIndex = new ProvisionedThroughput
        {
            ReadCapacityUnits = 1L,
            WriteCapacityUnits = 1L
        };

        var createIndex = new GlobalSecondaryIndex()
        {
            IndexName = "organization-index",
            ProvisionedThroughput = ptIndex,
            KeySchema = { new KeySchemaElement { AttributeName = "organization", KeyType = "HASH" } },
            Projection = new Projection { ProjectionType = "ALL" }
        };

        var createIndexClassName = new GlobalSecondaryIndex()
        {
            IndexName = "classname-index",
            ProvisionedThroughput = ptIndex,
            KeySchema = { new KeySchemaElement { AttributeName = "classname", KeyType = "HASH" } },
            Projection = new Projection { ProjectionType = "ALL" }
        };

        var Tbl = new CreateTableRequest
        {
            AttributeDefinitions = new List<AttributeDefinition>()
            {
                new AttributeDefinition
                {
                    AttributeName = "id",
                    AttributeType = "S"
                },
                new AttributeDefinition
                {
                    AttributeName = "classname",
                    AttributeType = "S"
                },
                new AttributeDefinition
                {
                    AttributeName = "organization",
                    AttributeType = "S"
                },
            },

            KeySchema = new List<KeySchemaElement>
            {
                new KeySchemaElement
                {
                    AttributeName = "id",
                    KeyType = "HASH"
                },
            },

            ProvisionedThroughput = new ProvisionedThroughput
            {
                ReadCapacityUnits = 3,
                WriteCapacityUnits = 3
            },

            TableName = tableName,
            GlobalSecondaryIndexes = {createIndex, createIndexClassName}
        };

        Client.CreateTableAsync(Tbl, (result) =>
        {
            if (result.Exception != null)
            {
                Debug.Log(result.Exception.Message);
                return;
            }

            var tableDescription = result.Response.TableDescription;
            Debug.LogFormat("Created {0}: {1} - ReadsPerSec: {2} - WritesPerSec: {3}\nAllow a few seconds for changes to reflect...",
                tableDescription.TableName,
                tableDescription.TableStatus,
                tableDescription.ProvisionedThroughput.ReadCapacityUnits,
                tableDescription.ProvisionedThroughput.WriteCapacityUnits);
        });
    }


    void DeleteTableVita(string tableName)
    {
        Client.DeleteTableAsync(tableName, (r) =>
        {
            if (r.Exception != null)
            {
                if (!r.Exception.Message.Contains("not found"))
                {
                    Debug.Log(r.Exception.Message.ToString());
                    //  return;
                }
            }

            Debug.LogFormat("DeleteTableVita('{0}') finished", tableName);
        });
    }


    public void AddEntity<T>(T o, CallbackResultDelegate callback)
    {
        Context.SaveAsync(o, (result) =>
        {
            string error = "";
            if (result.Exception != null)
            {
                error = "AddEntity() - Error Adding Entity: " + result.Exception.Message.ToString();
                Debug.LogError(error);
            }
            else
            {
                Debug.Log(typeof(T) + " Added");
            }

            if (callback != null)
                callback(null, error);
         });
    }

    public void DeleteEntity<T>(string hashKey, string rangeKey, CallbackResultDelegate callback)
    {
        if (rangeKey != null)
        {
            Context.DeleteAsync<T>(hashKey, rangeKey, (result) =>
            {
                string error = "";
                if (result.Exception != null)
                {
                    error = "DeleteEntity() - Error Deleting Entity: " + result.Exception.Message.ToString();
                    Debug.LogError(error);
                }
                else
                {
                    Debug.LogFormat("{0} Deleted: {1} - {2}", typeof(T), hashKey, rangeKey);
                }

                if (callback != null)
                    callback(null, error);
            });
        }
        else
        {
            Context.DeleteAsync<T>(hashKey, (result) =>
            {
                string error = "";
                if (result.Exception != null)
                {
                    error = "DeleteEntity() - Error Deleting Enity: " + result.Exception.Message.ToString();
                    Debug.LogError(error);
                }
                else
                {
                    Debug.LogFormat("{0} Deleted: {1}", typeof(T), hashKey);
                }

                if (callback != null)
                    callback(null, error);
            });
        }
    }

    public void BatchDelete<T>(List<T> items, CallbackResultDelegate callback)
    {
        string Result = "";
        try
        {
         
            Context.CreateBatchWriteAsync<T>((itemBatch) =>
            {
                itemBatch.Result.AddDeleteItems(items);
                itemBatch.Result.ExecuteAsync((res) =>
                {
            
                    if (res.Exception != null)
                    {
                        Result = ":ERROR: BatchDelete failed " + res.Exception.Message.ToString();
                        Debug.LogError(Result);

                        if (callback != null)
                            callback(null, Result);
                     }

                });            
            });
          }
        catch (Exception ex)
        {
            Result = ":ERROR: BatchDelete failed " + ex.Message.ToString();
            Debug.LogError(Result);

            if (callback != null)
                callback(null, Result);
        }

    }

    public void GetEntity<T>(string hashKeyValue, string rangeKeyValue, Dictionary<string, AttributeValue> lastKeyEvaluated, CallbackResultDelegate callback) where T : new()
    {
        // Get uniqely identified item

        if (!string.IsNullOrEmpty(hashKeyValue) && !string.IsNullOrEmpty(rangeKeyValue))
        {
            Context.LoadAsync<T>(hashKeyValue, rangeKeyValue, (result) =>
            {
                T data = new T();
                string error = "";

                if (result.Exception != null)
                {
                    error = "GetEntity() - :ERROR: " + result.Exception.ToString();
                    Debug.LogError(error);
                }
                else
                {
                    data = (T)result.Result;

                    if (data == null)
                    {
                        error = string.Format("GetEntity() - No item found: {0} - {1} - {2}", typeof(T), hashKeyValue, rangeKeyValue);
                        Debug.LogError(error);
                    }
                }

                if (callback != null)
                    callback(data, error);
            });
        }
        else if (!string.IsNullOrEmpty(hashKeyValue))
        {
            Context.LoadAsync<T>(hashKeyValue, (result) =>
            {
                T data = new T();
                string error = "";

                if (result.Exception != null)
                {
                    error = "GetEntity() - :ERROR: " + result.Exception.ToString() ;
                    Debug.LogError(error);
                }
                else
                {
                    data = (T)result.Result;

                    if (data == null)
                    {
                        error = string.Format("GetEntity() - No item found: {0} - {1}", typeof(T), hashKeyValue);
                        Debug.Log(error);
                    }
                }

                if (callback != null)
                    callback(data, error);
            });
        }
        else
        {
            if (callback != null)
                callback(null, "GetEntity() - Hash / Range Keys missing");
        }
    }

    public void GetAllEntities<T>(string tableName, Dictionary<string, AttributeValue> lastKeyEvaluated, CallbackResultDelegate callback) where T : new()
    {
        var request = new ScanRequest
        {
            TableName = tableName,
            Limit = 5000,
            ExclusiveStartKey = null,
        };

        Client.ScanAsync(request, (result) =>
        {
            List<T> data = new List<T>();
            string error = "";

            if (result.Exception != null)
            {
                error = "GetAllEntities() - :ERROR: Getting Entity " + typeof(T) + ": " + result.Exception.Message.ToString();
                Debug.LogError(error);
            }
            else
            {
                foreach (var item in result.Response.Items)
                {
                    T e = new T();
                    foreach (var kvp in item)
                    {
                        string attributeName = kvp.Key;

                        if (attributeName == "tableName")
                            continue;

                        System.Reflection.PropertyInfo pi = e.GetType().GetProperty(attributeName);
                        Type ty =  pi.PropertyType;

                        if (ty == typeof(int))
                        {
                            e.GetType().GetProperty(attributeName).SetValue(e, System.Convert.ToInt32(kvp.Value.N), null);
                        }
                        else if (ty == typeof(List<string>))
                        {
                            // when you get a string list from the DB, and the list is empty, the variable is null.
                            // for the user, we want an empty list, this makes the code a little cleaner
                            // so check this and fix it up before sending it to the user

                            List<string> items = kvp.Value.SS;
                            if (items == null) items = new List<string>();
                            e.GetType().GetProperty(attributeName).SetValue(e, items, null);
                        }
                        else if (ty == typeof(bool))
                        {
                            e.GetType().GetProperty(attributeName).SetValue(e, kvp.Value.BOOL, null);
                        }
                        else if (ty == typeof(string))
                        {
                            e.GetType().GetProperty(attributeName).SetValue(e, kvp.Value.S, null);
                        }
                    }

                    data.Add(e);
                }
            }

            lastKeyEvaluated = result.Response.LastEvaluatedKey;
            if (lastKeyEvaluated != null && lastKeyEvaluated.Count > 0)
            {
                GetAllEntities<T>(tableName, result.Response.LastEvaluatedKey, callback);
            }

            if (callback != null)
                callback((object)data, error);
        });
    }

    public void GetEntities<T>(string tableName, string indexName, string keyName, string keyValue, Dictionary<string, AttributeValue> lastKeyEvaluated, CallbackResultDelegate callback) where T : new()
    {
        // Uses indexes  (different from GetAllEntities)

        QueryRequest request = new QueryRequest();
        request.TableName = tableName;
        request.IndexName = indexName;
        request.ScanIndexForward = true;
        request.KeyConditions = new Dictionary<string, Condition>()
        {
            {
                keyName, new Condition()
                {
                    ComparisonOperator = "EQ",
                    AttributeValueList = new List<AttributeValue>()
                    {
                        new AttributeValue { S = keyValue }
                    }
                }
            }
        };

        Client.QueryAsync(request, (result) =>
        {
            List<T> data = new List<T>();
            string error = "";

            if (result.Exception != null)
            {
                error = "GetEntities() - :ERROR: Getting Entity " + typeof(T) + ": " + result.Exception.Message.ToString();
                Debug.LogError(error);
            }
            else
            {
                foreach (var item in result.Response.Items)
                {
                    T e = new T();
                    foreach (var kvp in item)
                    {
                        string attributeName = kvp.Key;

                        if (attributeName == "tableName")
                            continue;

                        System.Reflection.PropertyInfo pi = e.GetType().GetProperty(attributeName);
                        Type ty =  pi.PropertyType;

                        if (ty == typeof(int))
                        {
                            e.GetType().GetProperty(attributeName).SetValue(e, System.Convert.ToInt32(kvp.Value.N), null);
                        }
                        else if (ty == typeof(List<string>))
                        {
                            // when you get a string list from the DB, and the list is empty, the variable is null.
                            // for the user, we want an empty list, this makes the code a little cleaner
                            // so check this and fix it up before sending it to the user

                            List<string> items = kvp.Value.SS;
                            if (items == null) items = new List<string>();
                            e.GetType().GetProperty(attributeName).SetValue(e, items, null);
                        }
                        else if (ty == typeof(bool))
                        {
                            e.GetType().GetProperty(attributeName).SetValue(e, kvp.Value.BOOL, null);
                        }
                        else if (ty == typeof(string))
                        {
                            e.GetType().GetProperty(attributeName).SetValue(e, kvp.Value.S, null);
                        }
                    }

                    data.Add(e);
                }
            }

            lastKeyEvaluated = result.Response.LastEvaluatedKey;
            if (lastKeyEvaluated != null && lastKeyEvaluated.Count > 0)
            {
                GetEntities<T>(tableName, indexName, keyName, keyValue, result.Response.LastEvaluatedKey, callback);
            }

            if (callback != null)
                callback((object)data, error);
        });
    }




    public void  BatchAdd<T>(List<T> items, CallbackResultDelegate callback)
    {
        string Result = "";
      
        try{

            Context.CreateBatchWriteAsync<T>(itemBatch =>
            {
                itemBatch.Result.AddPutItems(items);              
            
                itemBatch.Result.ExecuteAsync(res =>
                {
                    if (res.Exception != null)
                    {
                        Result = ":ERROR: Batch Add failed " + res.Exception.Message.ToString();
                        Debug.LogError(Result);

                        if (callback != null)
                            callback(null, Result);
                    }                  
                 });       

            });

           }

           catch (Exception ex)
            {
             Result = ":ERROR: Batch Add failed " + ex.Message.ToString();
             Debug.LogError(Result);

             if (callback != null)
                callback(null, Result);
            }

      }


    public IEnumerator DeleteDatabase()
    {
        DeleteTableVita("VitaOrganization");             yield return new WaitForSeconds(5);
        DeleteTableVita("VitaProfile");                  yield return new WaitForSeconds(5);
        DeleteTableVita("VitaClass");                    yield return new WaitForSeconds(5);
        DeleteTableVita("VitaStudentSession");           yield return new WaitForSeconds(5);
        DeleteTableVita("VitaClassHomeworkAssigment");   yield return new WaitForSeconds(5);
        Debug.LogWarningFormat("DeleteDatabase() finished");
    }

    public IEnumerator CreateDatabase()
    {
        CreateTableVitaOrganization();         yield return new WaitForSeconds(5);
        CreateTableVitaProfile();              yield return new WaitForSeconds(5);
        CreateTableVitaClass();                yield return new WaitForSeconds(5);
        CreateTableVitaStudentSession();       yield return new WaitForSeconds(5);
        CreateTableClassHomeworkAssignment();  yield return new WaitForSeconds(5);
        Debug.LogWarningFormat("CreateDatabase() finished");
    }
}
