# Program

This program demonstrates a few concepts for auth in AspNetCore.

## Multiple Schemas

Schema is a way of identifying a user essentially. Think of it as Id, Passport and driving license in real life. Those are considered three different schemas.

## Policy

Policy is basically an array of requirements to be fulfiled to be considered authorized.

In the example, the policy `eu passport` requires the caller to be

1. An authenticated user
1. Being authenticated under the default scheme
1. Is more than 18 years old
1. Is having a claim named `passport` with value `eur`

If any of the above is unmatched, the policy fails and the user is unable to make the request.

## Endpoints Description

These are some info regarding the endpoints in the demo.

| | Endpoint | Description |
| --- | --- | --- |
| 1. | `/login_manual` | Manually assign `set-cookie` with data protected from custom IDP |
| 2. | `/login` | Logins as Sharon that fulfills the `eu passport` policy |
| 3. | `/username` | Tries to get the `usr` value from the claims |
| 4. | `/norway` | An endpoint that allows everyone to visit |
| 5. | `/denmark` | An endpoint that manually checks if the user is authenticated under the default scheme and having the `eur` value in the `passport` claim |
| 6. | `/sweden` | An endpoint that requires the user to fulfill the `eu passport` policy |

