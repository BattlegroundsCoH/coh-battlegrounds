using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace Battlegrounds.IntegrationTest;

public sealed record ServerIssueRecord(
    DateTimeOffset TimestampUtc,
    string Fixture,
    string Scenario,
    string Expected,
    string Actual,
    string? Details);

public static class ServerIssueReporter {

    private static readonly ConcurrentQueue<ServerIssueRecord> _issues = new();

    public static int IssueCount => _issues.Count;

    public static void Reset() {
        while (_issues.TryDequeue(out _)) {
            // Drain queue.
        }
    }

    public static void Report(string fixture, string scenario, string expected, string actual, string? details = null) {
        var issue = new ServerIssueRecord(
            TimestampUtc: DateTimeOffset.UtcNow,
            Fixture: fixture,
            Scenario: scenario,
            Expected: expected,
            Actual: actual,
            Details: details);

        _issues.Enqueue(issue);
        TestContext.Progress.WriteLine($"[SERVER-ISSUE] Fixture={fixture}; Scenario={scenario}; Expected={expected}; Actual={actual}; Details={details}");
    }

    public static IReadOnlyList<ServerIssueRecord> Snapshot() {
        return _issues.OrderBy(x => x.TimestampUtc).ToArray();
    }

    public static (string jsonPath, string markdownPath) WriteSummary(string outputDirectory, string filePrefix) {
        Directory.CreateDirectory(outputDirectory);

        string timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        string prefix = SanitizeFileName(filePrefix);
        string jsonPath = Path.Combine(outputDirectory, $"{prefix}.server-issues.{timestamp}.json");
        string markdownPath = Path.Combine(outputDirectory, $"{prefix}.server-issues.{timestamp}.md");

        var snapshot = Snapshot();

        var json = JsonSerializer.Serialize(snapshot, new JsonSerializerOptions {
            WriteIndented = true,
        });
        File.WriteAllText(jsonPath, json);

        var markdown = new StringBuilder();
        markdown.AppendLine("# Integration Server Issue Summary");
        markdown.AppendLine();
        markdown.AppendLine($"- Generated (UTC): {DateTimeOffset.UtcNow:O}");
        markdown.AppendLine($"- Fixture: {filePrefix}");
        markdown.AppendLine($"- Total Issues: {snapshot.Count}");
        markdown.AppendLine();

        if (snapshot.Count == 0) {
            markdown.AppendLine("No server issues were recorded in this test fixture run.");
        } else {
            foreach (var issue in snapshot) {
                markdown.AppendLine($"## {issue.Scenario}");
                markdown.AppendLine($"- Time (UTC): {issue.TimestampUtc:O}");
                markdown.AppendLine($"- Expected: {issue.Expected}");
                markdown.AppendLine($"- Actual: {issue.Actual}");
                if (!string.IsNullOrWhiteSpace(issue.Details)) {
                    markdown.AppendLine($"- Details: {issue.Details}");
                }
                markdown.AppendLine();
            }
        }

        File.WriteAllText(markdownPath, markdown.ToString());

        return (jsonPath, markdownPath);
    }

    private static string SanitizeFileName(string value) {
        if (string.IsNullOrWhiteSpace(value)) {
            return "integration";
        }

        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new string(value.Select(ch => invalidChars.Contains(ch) ? '_' : ch).ToArray());
        return sanitized;
    }
}
