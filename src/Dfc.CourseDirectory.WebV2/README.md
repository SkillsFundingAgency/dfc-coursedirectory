# dfc-coursedirectory - WebV2

## Patterns in use

### MediatR / CQRS

To achieve:

* thin-controllers,
* testability
* separation of concerns
* separation of testing from the aspnet infrastructure code

we are using [MediatR](https://github.com/jbogard/MediatR).

For an intro to MediatR in controllers see:

* https://codeopinion.com/thin-controllers-cqrs-mediatr/
    * [MediatR: Why should you use it? on YouTube](https://www.youtube.com/watch?v=yhpTZDavtsY)
* https://codeopinion.com/fat-controller-cqrs-diet-simple-query/
* https://ardalis.com/moving-from-controllers-and-actions-to-endpoints-with-mediatr/ 
* https://jonhilton.net/2016/06/06/simplify-your-controllers-with-the-command-pattern-and-mediatr/
* https://alexlindgren.com/posts/thin-controllers-using-mediatr-with-aspnet-mvc/

### OneOf

`OneOf<>` gives us better compile time checks for passing around one of multiple types. See the [OneOf readme](https://github.com/mcintyre321/OneOf/#readme) for further explanation.

### Tests

There are no controller tests because the controller actions are very thin mappings.

The tests are mostly integration tests that test an endpoint + behaviour.

### Multi-page transactions - FormFlow

To support stateful multi-page processes we are using the [FormFlow nuget package](https://www.nuget.org/packages/FormFlow/).

### Folder Structure

To keep things together that change together we are not using the traditional Views/Controllers folder structure. Instead we have `Features/*/` folders  that group together related functionality, including views, controllers and MediatR CQRS behaviour implementations.

### Feature-per-file

We are not doing class-per-file. Feature behaviour is implemented in a single .cs file following the MediatR query/command/handler patterns.
