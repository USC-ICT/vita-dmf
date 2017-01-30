using Amazon.DynamoDBv2.DataModel;
using System.Collections.Generic;
using System.Collections;
using System;

[Serializable]
[DynamoDBTable("VitaProfile")]
public class EntityDBVitaProfile
{
    // Use Hash and Range Key to Authenticate a user
    [DynamoDBHashKey]   // Hash key.
    public string           username        { get; set; }

    [DynamoDBRangeKey]  // Range Key
    public string           password        {get; set;}

    [DynamoDBProperty]
    public string           organization    {get; set;}

    [DynamoDBProperty]
    public int              type            { get; set; }  //0 --> root admin //1 --> organization admin  //2 -->teacher //3 -->student

    [DynamoDBProperty]
    public string           name            { get; set; }

    [DynamoDBProperty]
    public List<string>     badges          { get; set; }

    [DynamoDBProperty]
    public List<string>     unlocks         { get; set; }

    [DynamoDBProperty]
    public int              currency        { get; set; }

    [DynamoDBProperty]
    public List<string>     scores          { get; set; }

    [DynamoDBProperty]
    public string           createdate       {get;set;}

    [DynamoDBProperty]
    public string           expiredate      {get;set;}

    [DynamoDBProperty]
    public string           lastlogin       {get;set;}

    [DynamoDBProperty]
    public int              archived        {get; set;}

    [DynamoDBProperty]
    public List<string>     favorites       { get; set; }


    public EntityDBVitaProfile()
    {
    }

    public EntityDBVitaProfile(string username, string password, string organization, string name, int type)
    {
        this.username = username;
        this.password = password;
        this.organization = organization;
        this.name = name;
        this.type = type;
        this.favorites = new List<string>();
        this.badges = new List<string>();
        this.unlocks = new List<string>();
        this.currency = 0;
        this.scores = new List<string>();
        this.createdate = VitaGlobals.CurrentTimeToString();
        this.expiredate = VitaGlobals.MaxTimeToString();
        this.lastlogin = VitaGlobals.CurrentTimeToString();
        this.archived = 0;
    }

    public EntityDBVitaProfile(EntityDBVitaProfile user)
    {
        this.username = user.username;
        this.password = user.password;
        this.organization = user.organization;
        this.name = user.name;
        this.type = user.type;
        this.favorites = new List<string>(user.favorites);
        this.badges = new List<string>(user.badges);
        this.unlocks = new List<string>(user.unlocks);
        this.currency = user.currency;
        this.scores = new List<string>(user.scores);
        this.createdate = user.createdate;
        this.expiredate = user.expiredate;
        this.lastlogin = user.lastlogin;
        this.archived = user.archived;
    }

    public void FixNullLists()
    {
        // when you get a profile from the DB, and the list is empty, the variable is null.
        // for the user, we want an empty list, this makes the code a little cleaner
        // so check this and fix it up before sending it to the user

        if (this.favorites == null) this.favorites = new List<string>();
        if (this.badges == null)    this.badges = new List<string>();
        if (this.unlocks == null)   this.unlocks = new List<string>();
        if (this.scores == null)    this.scores = new List<string>();
    }
}
