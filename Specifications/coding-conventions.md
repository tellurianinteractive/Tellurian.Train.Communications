# Common C# code conventions

Coding conventions are essential for maintaining code readability, consistency, and collaboration within a development team. Code that follows industry practices and established guidelines is easier to understand, maintain, and extend. Most projects enforce a consistent style through code conventions. The [`dotnet/docs`](https://github.com/dotnet/docs) and [`dotnet/samples`](https://github.com/dotnet/samples) projects are no exception. In this series of articles, you learn our coding conventions and the tools we use to enforce them. You can take our conventions as-is, or modify them to suit your team's needs.

## Language guidelines

The following sections describe practices that we follows to write code. In general, follow these practices:
- Utilize modern language features and C# versions whenever possible.
- Avoid outdated language constructs.
- Only catch exceptions that can be properly handled; avoid catching general exceptions.
- Use specific exception types to provide meaningful error messages.
- Use LINQ queries and methods for collection manipulation to improve code readability.
- Use asynchronous programming with async and await for I/O-bound operations.
- Be cautious of deadlocks and use **System.Threading.Tasks.Task.ConfigureAwait(false)** when appropriate.
- Use the language keywords for data types instead of the runtime types. For example, use `string` instead of 'String' or `int` instead of 'Int32'. This recommendation includes using the types `nint` and `nuint`.
- Use `int` rather than unsigned types. The use of `int` is common throughout C#, and it's easier to interact with other libraries when you use `int`. Exceptions are for documentation specific to unsigned data types.
- Use `var` for temporary variables.
- Write code with clarity and simplicity in mind.
- Avoid overly complex and convoluted code logic.

More specific guidelines follow.

### String data
- Use **string interpolation** to concatenate short strings, as shown in the following code.
````csharp
string displayName = $"{nameList[n].LastName}, {nameList[n].FirstName}
````
- To append strings in loops, especially when you're working with large amounts of text, use a 'System.Text,StringBuilder' object.
````csharp
var phrase = "lalalalalalalalalalalalalalalalalalalalalalalalalalalalalala";
var manyPhrases = new StringBuilder();
for (var i = 0; i < 10000; i++)
{
    manyPhrases.Append(phrase);
}
//Console.WriteLine("tra" + manyPhrases);
````
- Prefer raw string literals to escape sequences or verbatim strings.
````csharp
var message = """
    This is a long message that spans across multiple lines.
    It uses raw string literals. This means we can 
    also include characters like \n and \t without escaping them.
    """;
````
- Use the expression-based string interpolation rather than positional string interpolation.
````csharp
// Execute the queries.
Console.WriteLine("scoreQuery:");
foreach (var student in scoreQuery)
{
    Console.WriteLine($"{student.Last} Score: {student.score}");
}
````

### Constructors and initialization
- Use Pascal case for primary constructor parameters on record types:
````csharp
public record Person(string FirstName, string LastName);
````
- Use camel case for primary constructor parameters on class and struct types.
- Use `required` properties instead of constructors to force initialization of property values:
````csharp
public class LabelledContainer<T>(string label)
{
    public string Label { get; } = label;
    public required T Contents 
    { 
        get;
        init;
    }
}
````

### Arrays and collections
- Use collection expressions to initialize all collection types:
````csharp
string[] vowels = [ "a", "e", "i", "o", "u" ];
````

### Delegates
- Use **Func<>** and **Action<>** instead of defining delegate types. In a class, define the delegate method.
````csharp
Action<string> actionExample1 = x => Console.WriteLine($"x is: {x}");

Action<string, string> actionExample2 = (x, y) =>
    Console.WriteLine($"x is: {x}, y is {y}");

Func<string, int> funcExample1 = x => Convert.ToInt32(x);

Func<int, int, int> funcExample2 = (x, y) => x + y;
````
- Call the method using the signature defined by the `Func<>` or `Action<>` delegate.
````csharp
actionExample1("string for x");

actionExample2("string for x", "string for y");

Console.WriteLine($"The value is {funcExample1("1")}");

Console.WriteLine($"The sum is {funcExample2(1, 2)}");
````
- If you create instances of a delegate type, use the concise syntax. In a class, define the delegate type and a method that has a matching signature.
````csharp
public delegate void aDelegate(string message);

public static void aMatchingMethod(string message)
{
    Console.WriteLine($"MatchingMethod argument: {message}");
}
````
- Create an instance of the delegate type and call it. The following declaration shows the condensed syntax.
````csharp
aDelegare exampleDelegate2 = aDelegateMethod;
exampleDelegate2("Hej");
````

### `try-catch` and `using` statements in exception handling

- Use a [try-catch](../../language-reference/statements/exception-handling-statements.md#the-try-catch-statement) statement for most exception handling.
````csharp
static double ComputeDistance(double x1, double y1, double x2, double y2)
{
    try
    {
        return Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
    }
    catch (System.ArithmeticException ex)
    {
        Console.WriteLine($"Arithmetic overflow or underflow: {ex}");
        throw;
    }
}
````
- Simplify your code by using the C# **using** statement. 
If you have a **try-finally** statement in which the only code in the `finally` block is a call to the System.IDisposable.Dispose method, use a `using` statement instead.
In the following example, the `try-finally` statement only calls `Dispose` in the `finally` block.
````csharp
Font bodyStyle = new Font("Arial", 10.0f);
try
{
    byte charset = bodyStyle.GdiCharSet;
}
finally
{
    bodyStyle?.Dispose();
}
````
- You can do the same thing with a `using` statement.
````csharp
using (Font arial = new Font("Arial", 10.0f))
{
    byte charset2 = arial.GdiCharSet;
}
````
- Use the new [`using` syntax](../../language-reference/statements/using.md) that doesn't require braces:
````csharp
using Font normalStyle = new Font("Arial", 10.0f);
byte charset3 = normalStyle.GdiCharSet;
````

### `&&` and `||` operators

- Use **&&** instead of **&** and **||** instead of **|** when you perform comparisons, as shown in the following example.
````csharp
Console.Write("Enter a dividend: ");
int dividend = Convert.ToInt32(Console.ReadLine());

Console.Write("Enter a divisor: ");
int divisor = Convert.ToInt32(Console.ReadLine());

if ((divisor != 0) && (dividend / divisor) is var result)
{
    Console.WriteLine($"Quotient: {result}");
}
else
{
    Console.WriteLine("Attempted division by 0 ends up here.");
}
````
If the divisor is 0, the second clause in the `if` statement would cause a run-time error. But the && operator short-circuits when the first expression is false. That is, it doesn't evaluate the second expression. The & operator would evaluate both, resulting in a run-time error when `divisor` is 0.

### `new` operator
- Use one of the concise forms of object instantiation when the variable type matches the object type, as shown in the following declarations. This form isn't valid when the variable is an interface type, or a base class of the runtime type.
````csharp
ExampleClass instance2 = new();
````

- The preceding declarations are equivalent to the following declaration.
````csharp
ExampleClass instance1 = new ExampleClass();
````
- Use object initializers to simplify object creation, as shown in the following example.
````csharp
var thirdExample = new ExampleClass { Name = "Desktop", ID = 37414,
    Location = "Redmond", Age = 2.3 };
````
- The following example sets the same properties as the preceding example but doesn't use initializers.
````csharp
var fourthExample = new ExampleClass();
fourthExample.Name = "Desktop";
fourthExample.ID = 37414;
fourthExample.Location = "Redmond";
fourthExample.Age = 2.3;
````

### Event handling

- Use a lambda expression to define an event handler that you don't need to remove later:

:::code language="csharp" source="./snippets/coding-conventions/program.cs" id="Snippet22":::

The lambda expression shortens the following traditional definition.

:::code language="csharp" source="./snippets/coding-conventions/program.cs" id="Snippet23":::

### Static members

Call [static](../../language-reference/keywords/static.md) members by using the class name: *ClassName.StaticMember*. This practice makes code more readable by making static access clear. Don't qualify a static member defined in a base class with the name of a derived class. While that code compiles, the code readability is misleading, and the code might break in the future if you add a static member with the same name to the derived class.

### LINQ queries

- Use meaningful names for query variables. The following example uses `seattleCustomers` for customers who are located in Seattle.

  :::code language="csharp" source="./snippets/coding-conventions/program.cs" id="Snippet25":::

- Use aliases to make sure that property names of anonymous types are correctly capitalized, using Pascal casing.

  :::code language="csharp" source="./snippets/coding-conventions/program.cs" id="Snippet26":::

- Rename properties when the property names in the result would be ambiguous. For example, if your query returns a customer name and a distributor name, instead of leaving them as a form of `Name` in the result, rename them to clarify `CustomerName` is the name of a customer, and `DistributorName` is the name of a distributor.

  :::code language="csharp" source="./snippets/coding-conventions/program.cs" id="Snippet27":::

- Use implicit typing in the declaration of query variables and range variables. This guidance on implicit typing in LINQ queries overrides the general rules for [implicitly typed local variables](#implicitly-typed-local-variables). LINQ queries often use projections that create anonymous types. Other query expressions create results with nested generic types. Implicit typed variables are often more readable.

  :::code language="csharp" source="./snippets/coding-conventions/program.cs" id="Snippet25":::

- Align query clauses under the [`from`](../../language-reference/keywords/from-clause.md) clause, as shown in the previous examples.

- Use [`where`](../../language-reference/keywords/where-clause.md) clauses before other query clauses to ensure that later query clauses operate on the reduced, filtered set of data.

  :::code language="csharp" source="./snippets/coding-conventions/program.cs" id="Snippet29":::

- Access inner collections with multiple `from` clauses instead of a [`join`](../../language-reference/keywords/join-clause.md) clause. For example, a collection of `Student` objects might each contain a collection of test scores. When the following query is executed, it returns each score that is over 90, along with the family name of the student who received the score.

  :::code language="csharp" source="./snippets/coding-conventions/program.cs" id="Snippet30":::

### Implicitly typed local variables

- Use [implicit typing](../../programming-guide/classes-and-structs/implicitly-typed-local-variables.md) for local variables when the type of the variable is obvious from the right side of the assignment.

  :::code language="csharp" source="./snippets/coding-conventions/program.cs" id="Snippet8":::

- Don't use [var](../../language-reference/statements/declarations.md#implicitly-typed-local-variables) when the type isn't apparent from the right side of the assignment. Don't assume the type is clear from a method name. A variable type is considered clear if it's a `new` operator, an explicit cast, or assignment to a literal value.

  :::code language="csharp" source="./snippets/coding-conventions/program.cs" id="Snippet9":::

- Don't use variable names to specify the type of the variable. It might not be correct. Instead, use the type to specify the type, and use the variable name to indicate the semantic information of the variable. The following example should use `string` for the type and something like `iterations` to indicate the meaning of the information read from the console.

  :::code language="csharp" source="./snippets/coding-conventions/program.cs" id="Snippet10":::

- Avoid the use of `var` in place of [dynamic](../../language-reference/builtin-types/reference-types.md). Use `dynamic` when you want run-time type inference. For more information, see [Using type dynamic (C# Programming Guide)](../../advanced-topics/interop/using-type-dynamic.md).

- Use implicit typing for the loop variable in [`for`](../../language-reference/statements/iteration-statements.md#the-for-statement) loops.

  The following example uses implicit typing in a `for` statement.

    :::code language="csharp" source="./snippets/coding-conventions/program.cs" id="Snippet7":::

- Don't use implicit typing to determine the type of the loop variable in [`foreach`](../../language-reference/statements/iteration-statements.md#the-foreach-statement) loops. In most cases, the type of elements in the collection isn't immediately obvious. The collection's name shouldn't be solely relied upon for inferring the type of its elements.

  The following example uses explicit typing in a `foreach` statement.

  :::code language="csharp" source="./snippets/coding-conventions/program.cs" id="Snippet12":::

- use implicit type for the result sequences in LINQ queries. The section on [LINQ](#linq-queries) explains that many LINQ queries result in anonymous types where implicit types must be used. Other queries result in nested generic types where `var` is more readable.

  > [!NOTE]
  > Be careful not to accidentally change a type of an element of the iterable collection. For example, it's easy to switch from <xref:System.Linq.IQueryable?displayProperty=nameWithType> to <xref:System.Collections.IEnumerable?displayProperty=nameWithType> in a `foreach` statement, which changes the execution of a query.

Some of our samples explain the *natural type* of an expression. Those samples must use `var` so that the compiler picks the natural type. Even though those examples are less obvious, the use of `var` is required for the sample. The text should explain the behavior.

### File scoped namespace declarations

Most code files declare a single namespace. Therefore, our examples should use the file scoped namespace declarations:

```csharp
namespace MySampleCode;
```

### Place the using directives outside the namespace declaration

When a `using` directive is outside a namespace declaration, that imported namespace is its fully qualified name. The fully qualified name is clearer. When the `using` directive is inside the namespace, it could be either relative to that namespace, or its fully qualified name.

```csharp
using Azure;

namespace CoolStuff.AwesomeFeature
{
    public class Awesome
    {
        public void Stuff()
        {
            WaitUntil wait = WaitUntil.Completed;
            // ...
        }
    }
}
```

Assuming there's a reference (direct, or indirect) to the <xref:Azure.WaitUntil> class.

Now, let's change it slightly:

```csharp
namespace CoolStuff.AwesomeFeature
{
    using Azure;

    public class Awesome
    {
        public void Stuff()
        {
            WaitUntil wait = WaitUntil.Completed;
            // ...
        }
    }
}
```

And it compiles today. And tomorrow. But then sometime next week the preceding (untouched) code fails with two errors:

```console
- error CS0246: The type or namespace name 'WaitUntil' could not be found (are you missing a using directive or an assembly reference?)
- error CS0103: The name 'WaitUntil' does not exist in the current context
```

One of the dependencies introduced this class in a namespace then ends with `.Azure`:

```csharp
namespace CoolStuff.Azure
{
    public class SecretsManagement
    {
        public string FetchFromKeyVault(string vaultId, string secretId) { return null; }
    }
}
```

A `using` directive placed inside a namespace is context-sensitive and complicates name resolution. In this example, it's the first namespace that it finds.

- `CoolStuff.AwesomeFeature.Azure`
- `CoolStuff.Azure`
- `Azure`

Adding a new namespace that matches either `CoolStuff.Azure` or `CoolStuff.AwesomeFeature.Azure` would match before the global `Azure` namespace. You could resolve it by adding the `global::` modifier to the `using` declaration. However, it's easier to place `using` declarations outside the namespace instead.

```csharp
namespace CoolStuff.AwesomeFeature
{
    using global::Azure;

    public class Awesome
    {
        public void Stuff()
        {
            WaitUntil wait = WaitUntil.Completed;
            // ...
        }
    }
}
```

## Style guidelines

In general, use the following format for code samples:

- Use four spaces for indentation. Don't use tabs.
- Align code consistently to improve readability.
- Limit lines to 65 characters to enhance code readability on docs, especially on mobile screens.
- Improve clarity and user experience by breaking long statements into multiple lines.
- Use the "Allman" style for braces: open and closing brace its own new line. Braces line up with current indentation level.
- Line breaks should occur before binary operators, if necessary.

### Comment style

- Use single-line comments (`//`) for brief explanations.
- Avoid multi-line comments (`/* */`) for longer explanations.<br/>Comments in the code samples aren't localized. That means explanations embedded in the code aren't translated. Longer, explanatory text should be placed in the companion article, so that it can be localized.
- For describing methods, classes, fields, and all public members use [XML comments](../../language-reference/xmldoc/index.md).
- Place the comment on a separate line, not at the end of a line of code.
- Begin comment text with an uppercase letter.
- End comment text with a period.
- Insert one space between the comment delimiter (`//`) and the comment text, as shown in the following example.

  :::code language="csharp" source="./snippets/coding-conventions/program.cs" id="Snippet3":::

### Layout conventions

Good layout uses formatting to emphasize the structure of your code and to make the code easier to read. Microsoft examples and samples conform to the following conventions:

- Use the default Code Editor settings (smart indenting, four-character indents, tabs saved as spaces). For more information, see [Options, Text Editor, C#, Formatting](/visualstudio/ide/reference/options-text-editor-csharp-formatting).
- Write only one statement per line.
- Write only one declaration per line.
- If continuation lines aren't indented automatically, indent them one tab stop (four spaces).
- Add at least one blank line between method definitions and property definitions.
- Use parentheses to make clauses in an expression apparent, as shown in the following code.

  :::code language="csharp" source="./snippets/coding-conventions/program.cs" id="Snippet2":::

Exceptions are when the sample explains operator or expression precedence.

## Security

Follow the guidelines in [Secure Coding Guidelines](../../../standard/security/secure-coding-guidelines.md).
