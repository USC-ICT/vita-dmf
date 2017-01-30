using Amazon.DynamoDBv2.DataModel;
using System;

[Serializable] 
[DynamoDBTable("VitaClassHomeworkAssigment")]
public class EntityDBVitaClassHomeworkAssigment
{
    [DynamoDBHashKey]
    public string          id { get; set; }

    [DynamoDBProperty]
    public string          name { get; set; }

    [DynamoDBProperty]
    public string          student { get; set; }

    [DynamoDBProperty]
    public string          organization    {get; set;}

    [DynamoDBProperty]
    public string          teacher      {get; set;}

    [DynamoDBProperty]
    public string          classname      {get; set;}

    [DynamoDBProperty]
    public string          description {get; set;}

    [DynamoDBProperty]
    public string          instructions {get; set;}

    [DynamoDBProperty]
    public string          studentresponse {get; set;}

    [DynamoDBProperty]
    public int             grade {get; set;}

    [DynamoDBProperty]
    public string          createdate   {get;set;}

    [DynamoDBProperty]
    public string          datedue    {get; set;} // assume that the date due is the same date for the whole class

    [DynamoDBProperty]
    public int             status      {get; set;}


    public static string tableName = "VitaClassHomeworkAssigment";


    public EntityDBVitaClassHomeworkAssigment()
    {
    }

    [Obsolete("This overload does not store due date properly")]
    public EntityDBVitaClassHomeworkAssigment(string name, string description, string instructions, string studentresponse, int grade, string organization, string teacher, string student, string classname, int status)
    {
        this.id = Guid.NewGuid().ToString();
        this.name = name;
        this.description = description;
        this.instructions = instructions;
        this.studentresponse = studentresponse;
        this.grade = grade;
        this.organization = organization;
        this.teacher = teacher;
        this.student = student;
        this.classname = classname;
        this.createdate = VitaGlobals.CurrentTimeToString();
        this.datedue = VitaGlobals.MaxTimeToString();
        this.status = status;
    }

    public EntityDBVitaClassHomeworkAssigment(string name, string description, string instructions, string studentresponse, int grade, string organization, string teacher, string student, string classname, string datedue, int status)
    {
        this.id = Guid.NewGuid().ToString();
        this.name = name;
        this.description = description;
        this.instructions = instructions;
        this.studentresponse = studentresponse;
        this.grade = grade;
        this.organization = organization;
        this.teacher = teacher;
        this.student = student;
        this.classname = classname;
        this.createdate = VitaGlobals.CurrentTimeToString();
        this.datedue = datedue;
        this.status = status;
    }

    public EntityDBVitaClassHomeworkAssigment(EntityDBVitaClassHomeworkAssigment assignment)
    {
        this.id = assignment.id;
        this.classname = assignment.classname;
        this.name = assignment.name;
        this.organization = assignment.organization;
        this.teacher = assignment.teacher;
        this.student = assignment.student;
        this.description = assignment.description;
        this.instructions = assignment.instructions;
        this.studentresponse = assignment.studentresponse;
        this.grade = assignment.grade;
        this.createdate = assignment.createdate;
        this.datedue = assignment.datedue;
        this.status = assignment.status;
    }


    public void FixNullLists()
    {
        // when you get a profile from the DB, and the list is empty, the variable is null.
        // for the user, we want an empty list, this makes the code a little cleaner
        // so check this and fix it up before sending it to the user
    }
}
