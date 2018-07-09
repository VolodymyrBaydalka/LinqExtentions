# LinqExtentions

[![zvsharp MyGet Build Status](https://www.myget.org/BuildSource/Badge/zvsharp?identifier=842b5ac9-096c-4d04-b592-c24e4af518a2)](https://www.myget.org/)


# Examples

    /// MVC controller 
    ActionResult GetItems(DataViewRequest request)
    {
      using(var context = new ItemsDbContext())
      {
        var data = context.Items.ToDataView(request)
        return Json(data);
      }
    }
    
    /// js
    $.get("/getItems?$take=20&$skip=3&$orderby=name asc").then(data => {
      $("#total").text(data.total);
      
      renderItems(data.items);
    });
