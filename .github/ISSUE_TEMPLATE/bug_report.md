---
name: Bug report
about: Create a report to help us improve
title: ''
labels: bug
assignees: salihkavaf

---

**Bug Report Template**

**Remember:**

- Please check that the [documentation](https://github.com/Primyer/Sqlist.NET/wiki) does not already explain the behaviour you are seeing.
- Please search both [open](https://github.com/Primyer/Sqlist.NET/issues) and [closed](https://github.com/Primyer/Sqlist.NET/issues?q=is%3Aissue+is%3Aclosed) issues to ensure your bug has not already been reported.

### **Describe the bug**

A clear and concise description of what the bug is.

### **Expected behaviour**

A clear and concise description of what you expected to happen.

### **Include your code**

To help us reproduce the issue, please attach a **small, runnable project** or provide a **minimal code listing** that reproduces the behaviour. It’s difficult for us to diagnose bugs from incomplete code snippets.

Use triple-tick code fences for code samples. Example:

\`\`\`csharp
Console.WriteLine("Hello, World!");
\`\`\`

### **Stack traces**

If the issue involves an exception, please provide the **full exception message** and **stack trace**.

Use triple-tick fences for stack traces. Example:

\`\`\`
Unhandled exception. System.NullReferenceException: Object reference not set to an instance of an object.
   at ExampleApp.Main() in C:\Path\ExampleApp.cs:line 15
\`\`\`

### **Verbose output**

Please include `--verbose` output when filing bugs about the `dotnet sqlist` tool.
Use triple-tick fences for tool output.

### **Versions (please complete the following):**

- **Project Version:** [e.g. 1.0.0]
- **Database Provider/Version:** (e.g. PostgreSQL)
- **Target Framework:** (e.g. .NET 8.0)
- **Operating System:** (e.g. Windows, Linux)
- **IDE:** (e.g. Visual Studio 2022, JetBrains Rider)
