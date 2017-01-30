using Amazon.DynamoDBv2.DataModel;
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;

[DynamoDBTable("VitaStudentSession")]
public class EntityDBVitaStudentSession
{
    [DynamoDBHashKey]
    public string           username{ get; set; }

    [DynamoDBRangeKey]
    public string           sessionname {get; set;}

    [DynamoDBProperty]
    public string           teacher    {get; set;}

    [DynamoDBProperty]
    public List<string>     events {get; set;}

    [DynamoDBProperty]
    public string           organization    {get; set;}

    [DynamoDBProperty]
    public bool             active      {get; set;}


    public static string tableName = "VitaStudentSession";


    public EntityDBVitaStudentSession()
    {
    }

    public EntityDBVitaStudentSession(string username, string sessionname, string teacher, string organization, bool active)
    {
        this.username = username;
        this.sessionname = sessionname;
        this.teacher = teacher;
        this.events = new List<string>();
        this.organization = organization;
        this.active = active;
    }


    public void FixNullLists()
    {
        // when you get a profile from the DB, and the list is empty, the variable is null.
        // for the user, we want an empty list, this makes the code a little cleaner
        // so check this and fix it up before sending it to the user

        if (this.events == null) this.events = new List<string>();
    }
}
