# Authentication

After the user logged in, the backend will return a response with `set-cookie` in the header with its associated cookie values.
After that, the user can call other endpoints with the cookie header to authenticate itself.

```cs
app.MapGet("/login", (HttpContext ctx) =>
{
	ctx.Response.Headers["set-cookie"] = "<key>=<value>";
	return "ok";
});

app.MapGet("/private", (HttpContext ctx) =>
{
	var cookie = ctx.Request.Headers.Cookie.FirstOrDefault(x => x.StartsWith("<key>="));
	// do something with cookie
});
```

## Improving Security: Encrypting Values

All the interactions above are happening in pure plain text, which might poses a security risk when sensitive information is involved.

To curb that security flaw, we can use encryption to secure the sensitive part when sending back the cookie response to the user.

The user then can use the encrypted cookie and call other endpoints and still get authenticated because the backend will know how to decrypt the cookie.

```cs
builder.Services.AddDataProtection();

app.MapGet("/login", (HttpContext ctx, IDataProtectionProvider idp) =>
{
	var protector = idp.CreateProtector("auth-cookie");
	ctx.Response.Headers["set-cookie"] = $"<key>={protector.Protect("<value>")}";
	return "ok";
});

app.MapGet("/secret", (HttpContext ctx, IDataProtectionProvider idp) =>
{
	var protector = idp.CreateProtector("auth-cookie");
	var cookie = ctx.Request.Headers.Cookie.FirstOrDefault(x => x.StartsWith("<key>="));
	var protectedPayload = cookie.Split("=").Last(); // '<value>'
	var payload = protector.Unprotect(protectedPayload);
	// do stuffs with decrypted payload
});
```

## Improving Reusability: Adding an AuthService

Up until now, the authentication is pretty tedious and manual. It would be difficult to change the authentication method this way.

We can centralize the logics into an authentication service and register it in the IoC container.

The `idp` can be accessed via interface `IDataProtectionProvider` and http context via `IHttpContextAccessor` from the constructor.

```cs
builder.Services.AddHttpContextAccessor();
```

## Decrypt Cookie in Middleware with `ClaimsPrincipal`

The `User` object can be populated in a middleware. A claim is a key-value pair that describes a user. It can be email, username etc.

The `ClaimsIdentity` contains a collection of claims of a user. It can be though of an official identity given by a government.

The `ClaimsPrincipal` is a set of documents that identify a user. It is the user himself.

```cs
app.Use((ctx, next) =>
{
	var idp = ctx.RequestServices.GetRequiredService<IDataProtectionProvider>();
	// -- snip --

	// who are you
	var claims = new List<Claim>
	{
		new("email", "sharon@gmail.com"),
		new("username", "sharon"),
	};

	// your official identity
	var identity = new ClaimsIdentity(claims);

	// documents
	ctx.User = new ClaimsPrincipal(identity);

	return next();
});
```

We can access the individual claims via the `FindFirst` method from a claims principal.

```cs
app.MapGet("/", (HttpContext ctx) =>
{
	return ctx.User.FindFirst("email").Value;
});
```

## Built-in Authentication Solution

Here is an example of using the built-in authentication with Cookie as the scheme.

Cookie can be thought of just an authentication scheme that identifies a user. Is it Facebook that identifies you? Is it Google? Or is it the government? Nope its Cookie.

The `AddCookie` method registers the necessary services to **read** and **write** to and from the encrypted cookie itself.

```cs
builder.Services.AddAuthentication("cookie")
	.AddCookie("cookie");
```

Next, we can use the built-in `SignInAsync` through the context for signing in.

```cs
app.MapGet("/login", async (HttpContext ctx) =>
{
	var authScheme = "cookie";
	List<Claim> claims = [new("username", "sharon")];
	var identity = new ClaimsIdentity(claims, authScheme);
	var user = new ClaimsPrincipal(identity);
	await ctx.SignInAsync(authScheme, user);
	return "ok";
});
```

# Authorization

To safe guard an API endpoint for those who are authorized only, we can utilize the Http context to check for required claims manually.

```cs
app.MapGet("/secret", (HttpContext ctx) =>
{
	// authentication
	if (!ctx.User.Identities.Any(x => x.AuthenticationType == "cookie")
	{
		ctx.Response.StatusCode = 401;
		return string.Empty;
	}

	// authorization
	if (!ctx.User.HasClaim("passport", "eur"))
	{
		ctx.Response.StatusCode = 403;
		return "Forbidden";
	}

	// authorized
	return "ok";
});
```

## Authorize Using Attributes

Since authorization can be quite repetitive across all endpoints, the better way is to put the logics in an attribute that will be invoked on each calls.

For example,

```cs
[AuthScheme("cookie")]
app.MapGet("/secret", (HttpContext ctx) =>
{
	// do stuffs
});
```

Besides, we can create attribute to verify the claims as well.

```cs
[AuthClaim("passport", "eur")]
app.MapGet("/norway", (HttpContext context) =>
{
	// do stuffs
}
```

## Authorize Using Middleware

Besides using attributes, authorization can be executed as a middleware too.

```cs
app.Use((ctx, next) =>
{
	// authentication
	if (!ctx.User.Identities.Any(x => x.AuthenticationType == "cookie")
	{
		ctx.Response.StatusCode = 401;
		return Task.CompletedTask;
	}

	// authorization
	if (!ctx.User.HasClaim("passport", "eur"))
	{
		ctx.Response.StatusCode = 403;
		return Task.CompletedTask;
	}

	return "ok";
});
```

Keep in mind that the middleware applies for **all** endpoints, including login or register.

Hence, we will need to circumvent all the kinds of auth checks for those endpoints.

```cs
app.Use((ctx, next) =>
{
	if (ctx.Request.Path.StartsWithSegments("/login"))
	{
		return next();
	}

	// -- snip --
});
```

## Built-in Authorization Solution

Microsoft have built in the authorization as well because it is used everywhere.

First, we will need to register the authorization policies.

```cs
builder.Services.AddAuthorization(/* register policy here */);

app.UseAuthorization();
```

A policy is a set of rules that can be applied to an endpoint.

The policy `eu passport` that requires a user to be authenticated, having the correct auth scheme and having a dedicated claims can be implemented as follows.

```cs
builder.Services.AddAuthorization(builder =>
{
	builder.AddPolicy("eu passport", pb =>
	{
		pb.RequireAuthenticatedUser()
			.AddAuthenticationSchemes("cookie")
			.RequireClaim("passport", "eur");
	});
});
```

After the policy `eu passport` is built, we can use it in any of the endpoints.

```cs
app.MapGet("/denmark", (HttpContext ctx) =>
{
	// ...
}).RequireAuthorization("eu passport");
```

The same applies to controller endpoints where we can utilize the `Authorize` attribute as follows.

```cs
[Authorize(policy = "eu passport")]
public async Task Secret() { }
```

To enable the public endpoints like login or register to allow unauthenticated access, we can use `AllowAnonymous`.

```cs
app.MapGet("/login", (HttpContext ctx) =>
{
	// ...
}).AllowAnonymous();
```

### Adding Requirements

We can extend the validation of a policy by using a requirement. It can be added as follows.

Each requirement requires two classes, one for accepting parameters, that will implements `IAuthorizationRequirement` and the other one is the validation class that will implements `AuthorizationHandler<TRequirement>`.

```cs [MinimumAgeRequirement.cs]
public class MinimumAgeRequirement : IAuthorizationRequirement
{
	private readonly int minimumAge;

	public MinimumAgeRequirement(int minimumAge)
	{
		this.minimumAge = minimumAge;
	}

	public int MinimumAge => minimumAge;
}
```

```cs [MinimumAgeHandler.cs]
public class MinimumAgeHandler : AuthorizationHandler<MinimumAgeRequirement>
{
	public MinimumAgeHandler() // Can inject any service that is registered
	{
	}

	protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, MinimumAgeRequirement requirement)
	{
		var configuredMinimumAge = requirement.MinimumAge;
		/*
			if success, call `context.Suceed(requirement)`
			optionally if fail, call `context.Fail(requirement)`

			always return Task.CompletedTask if not using Async services.
		 */
	}
}
```

The requirements can be registered in the IoC container.

```cs [Program.cs]
builder.Services.AddSingleton<IAuthorizationHandler, MinimumAgeHandler>();

builder.Services.AddAuthorization(builder =>
{
	builder.AddPolicy("eu passport", pb =>
	{
		pb.RequireAuthenticatedUser()
			.AddAuthenticationSchemes("cookie")
			.AddRequirements(new MinimumAgeRequirement(18))
			.RequireClaim("passport", "eur");
	});
});
```

