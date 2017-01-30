using Amazon.DynamoDBv2.DataModel;
using System.Collections.Generic;
using System.Collections;
using System;

[Serializable]
[DynamoDBTable("VitaOrganization")]
public class EntityDBVitaOrganization
{
    [DynamoDBHashKey]
    public string           name{ get; set; }

    [DynamoDBProperty]
    public string           username {get; set;}

    [DynamoDBProperty]
    public string           password {get; set;}

    [DynamoDBProperty]
    public string           firstname {get; set;}

    [DynamoDBProperty]
    public string           lastname {get; set;}

    [DynamoDBProperty]
    public string           email {get; set;}

    [DynamoDBProperty]
    public string           phone {get; set;}

    [DynamoDBProperty]
    public string           numteachers {get; set;}

    [DynamoDBProperty]
    public string           numstudents {get; set;}

    [DynamoDBProperty]
    public string           acccreated {get; set;}

    [DynamoDBProperty]
    public string           accexpire {get; set;}

    [DynamoDBProperty]
    public int              archived      {get; set;}


    public static string tableName = "VitaOrganization";


    public EntityDBVitaOrganization()
    {
    }

    public EntityDBVitaOrganization(string name, string firstname, string lastname, string email, string phone, string username, string password)
    {
        this.name        = name;
        this.firstname   = firstname;
        this.lastname    = lastname;
        this.email       = email;
        this.phone       = phone;
        this.username    = username;
        this.password    = password;
        this.numteachers = " ";  // needs to be generated
        this.numstudents = " ";  // needs to be generated
        this.acccreated  = VitaGlobals.CurrentTimeToString();
        this.accexpire   = VitaGlobals.MaxTimeToString();
        this.archived    = 0;
    }

    public EntityDBVitaOrganization(EntityDBVitaOrganization org)
    {
        this.name        = org.name;
        this.firstname   = org.firstname;
        this.lastname    = org.lastname;
        this.email       = org.email;
        this.phone       = org.phone;
        this.username    = org.username;
        this.password    = org.password;
        this.numteachers = org.numteachers;
        this.numstudents = org.numstudents;
        this.acccreated  = org.acccreated;
        this.accexpire   = org.accexpire;
        this.archived    = org.archived;
    }

    public void FixNullLists()
    {
        // when you get a profile from the DB, and the list is empty, the variable is null.
        // for the user, we want an empty list, this makes the code a little cleaner
        // so check this and fix it up before sending it to the user
    }
}
