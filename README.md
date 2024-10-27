# rubberduckvba.com
This solution contains the projects and resources that are hosted on a **Microsoft Azure** virtual machine that serves the [rubberduckvba.com](https://rubberduckvba.com) website.

- **rubberduckvba.client**  
  The typescript/Angular front-end client; the actual website.

- **rubberudckvba.Server**  
  The back-end server, served in production at [api.rubberduckvba.com](https://api.rubberduckvba.com).

- **rubberduckvba.database**  
  The SQL Server database holding site data. The database is accessed using two connection strings, making it possible to write to test or production databases from a local debug session with a local hangfire server.
  - **RubberduckDb** is used for storing all content under the `[dbo]` schema
  - **HangfireDb** is used for accessing the `[hangfire]` schema (scheduled tasks)
  
- **RubberduckServices**  
  Services involving Rubberduck 2.x libraries:
  - Rubberduck.Parsing.dll
  - Rubberduck.SmartIndenter.dll
  - Rubberduck.VBEditor.dll

___

# Local Setup
Microsoft SQL Server Express is required; double-click the `rubberduckvba.database.localdb.publish.xml` file under the database project to deploy the database schema locally. The Angular site should run out of the box, even without data.
  
  Updating GitHub metadata requires a GitHub access token; configure user secrets to define the following values:

```json
  "GitHub:OrgToken": "required",
  "GitHub:ClientSecretId": "optional",
  "GitHub:ClientId": "optional",
```

Where **OrgToken** is a valid _personal access token_ (PAT); generate one from your GitHub profile, under _developer settings_. **ClientId** and **ClientSecretId** are for OAuth login controllers, which was used by an older version of the site to grant certain content editing accesses to org members; create a GitHub OAuth application from your profile. The feature/content update pipelines use the PAT for credentials instead.
___

# Hangfire Tasks
The server uses Hangfire to schedule and run background tasks, and creates two jobs at startup:
- **update_installer_downloads** is scheduled to run on a daily basis
- **update_xmldoc_content** is created but not scheduled

## Installer download stats
When triggered, the **update_installer_downloads** job creates a TPL DataFlow pipeline that can be documented as follows:

![diagram](https://github.com/user-attachments/assets/a1b81d49-3bfa-4660-b399-7da2a39a2538)

Essentially, we're hitting GitHub (using the **GitHub:OrgToken** credentials) to retrieve all tags from the **rubberduck-vba/Rubberduck** repository. Once we have all the tags, we identify the current **release** (main) and **pre-release** (next) tags and proceed to retrieve the xmldoc assets from these tags, and then we either update existing records with the updated data, or we insert new records with the new data - either way the website's front page displays updated figures and an updated timestamp immediately after it completes.

The task can be launched manually from the back-end API with an authenticated POST request to `/admin/update/tags`.

## Update XmlDoc content
Similarly, this job creates a TPL DataFlow pipeline that can be documented as follows:

![diagram](https://github.com/user-attachments/assets/519e1d61-514f-4186-a694-4db69d7e8da9)

This pipeline hits GitHub to download the code inspections' default configuration from the **rubberduck-vba/Rubberduck** repository, fetches the current-latest tags from the database to compare against the current-latest tags from GitHub, then downloads the .xml assets and parses them into "feature items" and proceeds to merge the contents:
- Items that exist in **next** but not **main** are considered/marked as NEW
- Items that exist in **main** but not in **next** are considered/marked as DISCONTINUED
- Items that exist in both branches get their content from **next**
- Items for which the json-serialized data has changed get updated; the others are left alone.

The task can be launched manually from the back-end API with an authenticated POST request to `/admin/update/xmldoc`; the endpoint is intended to be invoked by a webhook listening for GitHub tag/release activity, automatically updating the inspections, quickfixes, and annotations content accordingly.
