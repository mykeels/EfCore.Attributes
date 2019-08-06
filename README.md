# EfCore.Attributes

This dotnet package contains custom validation attributes, and action filters for making sure models are validated with the database.

## Attributes

- `[Exists]` Attribute

When applied to an action parameter, or to a model field, ASP.NET makes sure an entry exists in the database with that parameter/field's value as either its primary key, or in the specified column.

- `[Unique]` Attribute

When applied to an action parameter, or to a model field, ASP.NET makes sure no entry exists in the database with that parameter/field's value as either its primary key, or in the specified column.

- `[ValidateDb]` Attribute

When applied to an action or controller, ASP.NET makes sure that all ef validation attributes on the action's parameters, or their subfields, are enforced.

## Usage

1. Using Nuget, install `EfCore.Attributes`.

```bash
dotnet add package EfCore.Attributes
```

1. Include the namespace in your class.

```cs
using EfCore.Attributes;
```

1. Add the validation attributes to a controller action parameter, or a model field.

```cs
[ValidateDB(DbContext)]
public ActionResult Demo([Exists(typeof(User))]Guid id)
```

```cs
using EfCore.Attributes;

public class StaffInputModel {
    [Exists]
    public Guid Id { get; set; }

    [Unique("Staff", "BadgeNo")]
    public string BadgeNo { get; set; }
}
```

1. Add the action filter to the controller or action.

```cs
[ValidateDB(DbContext)]
public ActionResult Demo([FromBody]StaffInputModel model)
```
