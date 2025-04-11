# Concerto Interview - Technical Task

********************************

## For Internal Use

See internal use notes here: https://bellrockgroup.atlassian.net/wiki/spaces/Concerto/pages/113246209/Interview+Technical+Task

********************************

## The Task

Implement an API that can be used to manage the inventory of a small shop in London.

The API should be able to fulfil the data requirements in the [images folder](images), with support for the following capabilities:

- List all items
- Get a single item
- Create a new item
- Update an item
- Delete an item

In addition to this, the following business rules should be added to the solution:

- When the quantity of stock for an item is greater than 5, the price should be discounted by 10%
- When the quantity of stock for an item is greater than 10, the price should be discounted by 20%
- Every Monday between 12pm and 5pm, all items are discounted by 50%
- Only a single discount should be applied to an item at any time, the highest discount percentage

### Getting Started

Ensure you have the .NET 7.0 SDK installed, and your IDE of choice.

Clone this repository, then run the project either in your IDE, or via the CLI command below while in the solution folder.

`dotnet run --project DotNetInterview.API`

Swagger UI is installed and available at `/swagger/index.html`, where you can interact with your API endpoints.

### Data

An existing `DbContext` named `DataContext.cs` has been added to the project, and is seeded with dummy data on application startup.
This should be sufficient to allow you to test API endpoints and business logic effectively.

## Your Solution

Your submission should be a Git repository we can clone, containing your implementation. We expect it to build and for any tests to be passing.

We are looking for a solution that is easy to understand and well tested. We'll evaluate your approach, design patterns and coding style, but are equally as interested in the process that you go through
to develop the code as the end result. So, commit often so we can see the steps you go through to arrive at your solution.

## User Interface Example

For an example of what the consuming user interface might look like, see the image here: `/Images/UI.png`
