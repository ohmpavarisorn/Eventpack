# Deploy EventPack on a free hosting plan

The original ASP.NET Core app runs from the root Dockerfile. The `docs/` folder is the independent, browser-only GitHub Pages demo.

1. Create an Aiven for PostgreSQL service on the Free plan. Use an empty database for this portfolio instance. The Aiven service URI can be used directly and connects over TLS. For certificate verification, add the Aiven CA as a Render secret file and set `Database__RootCertificate=/etc/secrets/aiven-ca.pem`.
2. Create a Render Free Web Service using the repository and the Docker runtime. Select this branch until the changes are merged. Set the health check path to `/healthz`. The `render.yaml` also defines these settings for a Blueprint.
3. Set the following values in Render's environment settings. Keep credentials in the provider settings.

| Variable | Value |
| --- | --- |
| `Database__Provider` | `PostgreSQL` for Aiven Free. Omit this to use the original MySQL provider. |
| `DATABASE_URL` | PostgreSQL service URI for the empty database. For MySQL or a native Npgsql connection string, set `ConnectionStrings__DefaultConnection` instead. |
| `Admin__UserId` | Owner account name of 1–10 characters |
| `Admin__InitialPassword` | Initial password of at least 12 characters. Used only to create the owner account on first start. |

The first start creates the schema and sample dishes in the empty database. The admin account is accessible only by the configured owner. Keep the admin username configured and remove the initial password variable after confirming the account works.

## Verify the two roles

- Customer: register a test account, sign in, select dishes, enter a future date and sample venue and phone, and submit the booking. Check the quote, confirmation, and My Bookings. Use a second booking to verify customer cancellation while pending.
- Admin: sign in as the configured owner. Filter bookings by status, confirm or cancel a pending booking, and add or edit a dish. Return to the customer account to see the booking status and verify that a changed menu price applies only to new bookings.
- Access: a customer can open only their own booking. A customer account cannot open the admin dashboard or change menu prices.

Use fictional contact details when testing. The application does not take payments.
