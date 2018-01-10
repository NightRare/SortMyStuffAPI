# SortMyStuff API

SortMyStuffAPI is a REST style API that conforms to the HATEOAS constraint. It encapsulates the data access layer of SortMyStuff and ready to serve various front-end applications. The API supports pagination, searching and ordering for collection responses. It is built with ASP.NET Core 2.0, deployed on Azure, backed by SQL Server for relational data and Firebase storage for BLOB data.

## Example Results

The JSON response conforms to the [Ion Specification](https://ionwg.org/).

- `GET categories`

  ```json
  {
      "href": "https://sortmystuffapi.azurewebsites.net/categories",
      "rel": [
          "collection"
      ],
      "offset": 0,
      "pageSize": 10000,
      "size": 2,
      "first": {
          "href": "https://sortmystuffapi.azurewebsites.net/categories",
          "rel": [
              "collection"
          ]
      },
      "value": [
          {
              "href": "https://sortmystuffapi.azurewebsites.net/categories/38bc818b-2fc2-441c-954d-9bf365d39409",
              "name": "Books",
              "baseDetails": {
                  "href": "https://sortmystuffapi.azurewebsites.net/basedetails?search=categoryId%20eq%2038bc818b-2fc2-441c-954d-9bf365d39409",
                  "rel": [
                      "collection"
                  ]
              },
              "categorisedAssets": {
                  "href": "https://sortmystuffapi.azurewebsites.net/assets?search=categoryId%20eq%2038bc818b-2fc2-441c-954d-9bf365d39409",
                  "rel": [
                      "collection"
                  ]
              },
              "formSpecs": {
                  "href": "https://sortmystuffapi.azurewebsites.net/docs/categories/38bc818b-2fc2-441c-954d-9bf365d39409",
                  "rel": [
                      "collection"
                  ]
              }
          },
          {
              "href": "https://sortmystuffapi.azurewebsites.net/categories/2543d65d-456f-4be9-9e41-310a946d97a8",
              "name": "Appliances",
              "baseDetails": {
                  "href": "https://sortmystuffapi.azurewebsites.net/basedetails?search=categoryId%20eq%202543d65d-456f-4be9-9e41-310a946d97a8",
                  "rel": [
                      "collection"
                  ]
              },
              "categorisedAssets": {
                  "href": "https://sortmystuffapi.azurewebsites.net/assets?search=categoryId%20eq%202543d65d-456f-4be9-9e41-310a946d97a8",
                  "rel": [
                      "collection"
                  ]
              },
              "formSpecs": {
                  "href": "https://sortmystuffapi.azurewebsites.net/docs/categories/2543d65d-456f-4be9-9e41-310a946d97a8",
                  "rel": [
                      "collection"
                  ]
              }
          }
      ]
  }
  ```

- `GET assets/{assetid}`

  ```json
  {
    "href": "https://sortmystuffapi.azurewebsites.net/assets/b0d5366e-db12-4061-bbbb-2e6861ece4e3",
    "name": "Being and Time",
    "createTimestamp": "2018-01-10T08:56:48.972402+00:00",
    "modifyTimestamp": "2018-01-10T08:56:48.972402+00:00",
    "category": {
        "href": "https://sortmystuffapi.azurewebsites.net/categories/38bc818b-2fc2-441c-954d-9bf365d39409"
    },
    "path": {
        "href": "https://sortmystuffapi.azurewebsites.net/assets/b0d5366e-db12-4061-bbbb-2e6861ece4e3/path",
        "rel": [
            "collection"
        ]
    },
    "contentAssets": {
        "href": "https://sortmystuffapi.azurewebsites.net/assets?search=containerId%20eq%20b0d5366e-db12-4061-bbbb-2e6861ece4e3",
        "rel": [
            "collection"
        ]
    },
    "assetTree": {
        "href": "https://sortmystuffapi.azurewebsites.net/assettrees/b0d5366e-db12-4061-bbbb-2e6861ece4e3"
    },
    "thumbnail": {
        "href": "https://sortmystuffapi.azurewebsites.net/thumbnails/b0d5366e-db12-4061-bbbb-2e6861ece4e3.jpg"
    },
    "photo": {
        "href": "https://sortmystuffapi.azurewebsites.net/photos/b0d5366e-db12-4061-bbbb-2e6861ece4e3.jpg"
    },
    "details": {
        "href": "https://sortmystuffapi.azurewebsites.net/details?search=assetId%20eq%20b0d5366e-db12-4061-bbbb-2e6861ece4e3",
        "rel": [
            "collection"
        ]
    },
    "formSpecs": {
        "href": "https://sortmystuffapi.azurewebsites.net/docs/assets/b0d5366e-db12-4061-bbbb-2e6861ece4e3",
        "rel": [
            "collection"
        ]
    }
  }
  ```

## Demo

As this API is not designed for public use, it would be somewhat confusing when testing it without a context.

But you are also welcome to play with it:

1. Go to https://sortmystuffapi.azurewebsites.net/token and **POST** an `x-www-form-urlencoded` (specify in header) request with the following data:

    | Key        | Value           |
    | ---------- | --------------- |
    | grant_type | password        |
    | username   | test@yyuan.tech |
    | password   | Test1234.       |

    _Note: The period is the last character of the password string. Don't leave it out._

1. Copy the VALUE of the **access_token** and use it in the header of **EVERY** other request. An example of the header:

    | Key        | Value           |
    | ---------- | --------------- |
    | Authorization | Bearer {the_access_token_value} |

    _Note: Please keep one space between "Bearer" and the access token value._

1. Start testing (a good start would be `GET https://sortmystuffapi.azurewebsites.net/`). The API follows HATEOAS, most of the responses are sort of "self-documented".

As my Azure is subscribed with the MS Imagine Subscription (free student subscription), the performance of the host and SQL Server is not very ideal. Slow response would be expected sometimes.
