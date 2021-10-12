# Dapper.Alpha
[![NuGet](https://img.shields.io/nuget/v/Dapper.Alpha.svg)](https://www.nuget.org/packages/Dapper.Alpha)
[![NuGet](https://img.shields.io/nuget/dt/Dapper.Alpha.svg)](https://www.nuget.org/packages/Dapper.Alpha)
[![License MIT](https://img.shields.io/badge/license-MIT-green.svg)](https://opensource.org/licenses/MIT)

## Description
If you like your code to run fast, you probably know about Micro ORMs.
They are simple and one of their main goals is to be the fastest execution of your SQL sentences in you data repository.
For some Micro ORM's you need to write your own SQL sentences and this is the case of the most popular Micro ORM [Dapper](https://github.com/DapperLib/Dapper)

This tool abstracts the generation of the SQL sentence for CRUD operations based on each C# POCO class "metadata".
We know there are plugins for both Micro ORMs that implement the execution of these tasks, but that's exactly where this tool is different. The "SQL Generator" is a generic component
that generates all the CRUD sentences for a POCO class based on its definition and the possibility to override the SQL generator and the way it builds each sentence.

The original idea was taken from [phnx47](https://github.com/phnx47/dapper-repositories).
## Features
- Support for MsSql, MySql, PostgreSql, Sqlite.
- Support soft Deleted, BulkInsert, BulkUpdate.
- A useful SQL builder and statement formatter which can be used even if you don't need the CRUD features of this library.
- Implement the repository and unit of work patterns.
- Fast pre-computed entity queries

## Installation

```sh
dotnet add package Dapper.Alpha
```

## Docs
### Metadata attributes
This is the set of attributes to specify POCOs metadata. All of them are under the `Dapper.Alpha.Attributes` namespace:

**[Key]**  
- Use for primary key.

**[Identity]**  
- Use for identity key.

**[Table]**  
- By default the database table name will match the model name but it can be overridden with this.

**[Column]**  
- By default the column name will match the property name but it can be overridden with this.

**[NotMapped]**  
- For "logical" properties that do not have a corresponding column and have to be ignored by the SQL Generator.

**[Computed]**  
- For "logical" properties that do not have a corresponding column and have to be ignored by the Insert SQL Generator.

**[Deleted], [Status]**  
- For tables that implement "logical deletes" instead of physical deletes. Use this to decorate the `bool` or `enum`.
- If Status has `IsEnumDbString = true` then sql will compare the column with the `Enum.ValueDeleted.ToString()`. Default use Enum with Status logical delete is Int.

 ### Where predicate function suport
 - Contains
 - StringContains
 - CompareString
 - Equals
 - StartsWith
 - EndsWith
 
 ### Maps

"Users" POCO:

```c#
[Table("Users")]
public class User
{
    [Key, Identity]
    public int Id { get; set; }
    
    [Status]
    public SoftDeletedStatus Status { get; set; }
    
    [Column("Username")]
    public string Email { get; set; }

    public string Password { get; set; }
}
```
```c#
public enum SoftDeletedStatus
{
    Inactive = 0,

    Active = 1,

    [Deleted]
    Deleted = -1
}
```
### Example query

.NET Core uses dependency injection config
```c#
public void ConfigureServices(IServiceCollection services)
{
  services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>(option =>
    new DbConnectionFactory(Configuration.GetSection(nameof(AppSettings))[nameof(AppSettings.ConnectionString)], Metadata.SqlDialect.MsSql)
  );
  services.AddScoped<IUnitOfWork, UnitOfWork>();
}
```
FindById:

```c#
var userRepository = unitOfWork.GetRepository<User>();
var user = userRepository.FindById(5);
//Or Use var user = userRepository.FindById(x => x.Id == 5); 
```  

FindAll:

```c#
var userRepository = unitOfWork.GetRepository<User>();
var users = userRepository.FindAll();
//FindAll with expression
//var users = userRepository.FindAll(o=> o.Email == "everything@gmail.com");
```  


Insert:

```c#
var userRepository = unitOfWork.GetRepository<User>();
var user = new User()
{
 Email = "everything@gmail.com",
 Password = "123456"
};
bool isSuccess = userRepository.Insert(user);
//int userId = userRepository.Insert<int>(user);
```

Update:

```c#
var userRepository = unitOfWork.GetRepository<User>();
var user = userRepository.FindById(1);
user.Password = "1234566";
bool isSuccess = userRepository.Update(user);
//Update any feild
//bool isSuccess = userRepository.Update(user, property => property.Password);
```
BulkInsert:

```c#
var userRepository = unitOfWork.GetRepository<User>();
var users = new List<User>();
users.Add(new User(){
  Email = "email1@gmail.com",
  Password = "123456"
});
users.Add(new User(){
  Email = "email2@gmail.com",
  Password = "123456"
});
userRepository.BulkUpdate(users);
```

BulkUpdate:

```c#
var userRepository = unitOfWork.GetRepository<User>();
var users = userRepository.FindAll();
users.All(user=>{
  user.Status = SoftDeletedStatus.Deleted;
});
bool isSuccess = userRepository.BulkUpdate(users);
```

## Buy me a cup coffee
![donate-paypal](https://user-images.githubusercontent.com/44061499/136683350-9176a5e9-b0f5-42b8-a809-1021a8141cc0.png)
![donate-momo](https://user-images.githubusercontent.com/44061499/136800805-29321b18-eceb-41ee-87b1-5c7e8a7ad81b.png)
![donate-btc](https://user-images.githubusercontent.com/44061499/136683352-53dde31c-6a7e-45d3-ad77-d7d86876a456.png)
![donate-eth](https://user-images.githubusercontent.com/44061499/136683356-239a9537-ecc9-414c-8acd-2dcace3e10b7.png)
![donate-trx](https://user-images.githubusercontent.com/44061499/136683362-5adad679-25cb-4aec-84c5-461ca3844120.png)

**Paypal:** https://www.paypal.com/paypalme/nghiaphamqn

**Momo:** https://nhantien.momo.vn/fexx4gvBN0c

**BTC:** bc1qwxcfpgwn6qzm67munujmaavcfqmueht2wdz8v7

**TRX:** TDASKEHF4HPGw9u48exLAvRVXo8YQmKHH7

**ETH & BSC & Heco:** 0x230810F96299E6172e564809f600A592e2a81000

![image](https://user-images.githubusercontent.com/44061499/136688224-3aa3af74-8bff-4a31-b0d5-e510a31ccf67.png)
## License

All contents of this package are licensed under the [MIT license](https://opensource.org/licenses/MIT).
