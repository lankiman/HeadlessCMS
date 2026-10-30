# Prompts for the AI agent

Paste one of these into your agent. Fill in the parts in `<angle brackets>`.
The agent reads `AGENTS.md` for the standing rules, so these only say what to do this time.

## 1. Build everything around a feature

```
Read AGENTS.md and docs/PLAN.md first.

Feature: <for example: POST /auth/register>
Service interface: <the interface the endpoint calls, or "define it">
Rules for the request: <for example: email required and valid; password at least 12 characters>
What the response contains: <fields. Never include secrets.>

I write the security-critical logic myself. Do everything around it:
1. Request and response DTO records in HeadlessCMS.API/Contracts/<Feature>/.
2. FluentValidation validators next to them.
3. Manual mapping extension methods in HeadlessCMS.API/Mapping/.
4. The endpoint, calling the service interface only.
5. OpenAPI metadata: a summary, and every response status it can return.
6. xunit tests: success, each validation rule, and each failure path.
7. XML comments on public members, and an entry for the endpoint in docs/API.md.

Follow AGENTS.md exactly. Run dotnet build and dotnet test, and fix until both pass.
When done, list the files you changed and anything you were unsure about.
Do not change anything outside this feature.
```

## 2. Tests only

```
Read AGENTS.md first.

Write tests for <class or feature> in the matching folder under HeadlessCMS.Tests.
Cover the success path, every validation rule, and every failure path.
Do not change non-test code. If you find a bug, stop and report it with a failing test.
Run dotnet test and report the result.
```

## 3. Documentation only

```
Read AGENTS.md first.

Document <feature or area>: XML comments on every public type and member, an entry in docs/API.md
(route, request, response, status codes, an example), and any README changes for setup.
Describe what the code does today, not what it should do. Change no code except comments.
```

## 4. Review before I merge

```
Read AGENTS.md first.

Review the changes on this branch against AGENTS.md. Look for secrets in logs or responses,
a missing workspace filter, a missing soft-delete filter, layering mistakes, and missing tests.
List the problems by severity. Do not change any code.
```
