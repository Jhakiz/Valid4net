# Valid4net

Manage easyly your models validations by customizing different properties rules.

# How to use

```sh
PM> Install-Package Valid4net
```

```sh
> dotnet add package Valid4net
```

The library contains a class named **"Valid4netObject"** which your project model has to inherit and access to this base class methods, properties, etc.

#### 1. Model inheritance
````csharp
 public class Product : Valid4netObject<Product> {
     // TODO
 }
````
#### 2. Properties definition
````csharp
 public class Product : Valid4netObject<Product> {
    private string _id;

    public string Id
    {
        get => _id; 
        set
        { 
            SetProperty(ref _id, value); 
        }
    }
 }
````

**Note:** The function `SetProperty(ref _id, value);` will notify property changes to apply rules and is needed for each property which needs ***validations***.

#### 3. Model rules definition

### - Using contructor
  
````csharp
    ...
    public Product()
    {
        Rules.Add(nameof(Id), "Id cannot be empty!!", p => !string.IsNullOrEmpty(p.Id));
    }
    ...
````
### - Using model instance
````csharp
    ...
    // Object model
    var product = new Product();

    // Rules definition
    product.AddRule(nameof(product.Id), "Id cannot be empty!!", p => !string.IsNullOrEmpty(p.Id));
    product.AddRule(nameof(product.Title), "Title length must be between 5 and 10!!",p => (p.Title?.Length > 5) && (p.Title?.Length <= 20));
    ...
````

#### 4. Test if model is valid

  ````csharp
    ...
	Product product = new Product();
	bool result = product.HasErrors;
	...
  ````

#### 5. Get errors

  ````csharp
    ...
	// Get whole model errors
	product.GetErrors();

	// Get single property errors
	product.GetErrors("Title");
	...
  ````
  
