# JWT Authentication Example

## Projects

- IdentityApi: Exposes `/login` endpoint to retrieve a JWT token.
- WeatherApi: Exposes `/weatherforecast` endpoint to retrieve weather data. It requires a JWT token to access.

## Notes

If we have a User object, we can add `Role` property to it and scope any endpoints with the `Authorize` attribute for RBAC. Of course, the `Role` needs to be added in the claims.

```cs
[Authorize(Roles = "Admin,User")]
```

