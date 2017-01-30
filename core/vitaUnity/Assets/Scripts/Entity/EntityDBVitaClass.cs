using Amazon.DynamoDBv2.DataModel;
using System.Collections.Generic;
using System.Collections;
using System;

[Serializable]
[DynamoDBTable("VitaClass")]
public class EntityDBVitaClass
{
    [DynamoDBHashKey]
    public string         classname { get; set; }

    [DynamoDBProperty]
    public string         teacher{get; set;}

    [DynamoDBProperty]
    public string         organization {get; set;}

    [DynamoDBProperty]
    public List<string>   students { get; set; }

    [DynamoDBProperty]
    public string         datecreated { get; set; }


    public static string tableName = "VitaClass";


    public EntityDBVitaClass()
    {
    }

    public EntityDBVitaClass(string classname, string organization)
    {
        this.classname = classname;
        this.teacher = " ";
        this.organization = organization;
        this.students = new List<string>();
        this.datecreated = VitaGlobals.CurrentTimeToString();
    }

    public EntityDBVitaClass(EntityDBVitaClass newClass)
    {
        this.classname = newClass.classname;
        this.teacher = newClass.teacher;
        this.organization = newClass.organization;
        this.students = new List<string>(newClass.students);
        this.datecreated = newClass.datecreated;
    }

    public void FixNullLists()
    {
        // when you get a profile from the DB, and the list is empty, the variable is null.
        // for the user, we want an empty list, this makes the code a little cleaner
        // so check this and fix it up before sending it to the user

        if (this.students == null) this.students = new List<string>();
    }
}
