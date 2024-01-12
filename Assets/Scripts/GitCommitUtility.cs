using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

// Adapted from gist by Raphaël Daumas https://gist.github.com/Marsgames/219afc01a1b6af1ed20c241ff449e574
public static class GitCommitUtility
{
    private static string RunGitCommand(string gitCommand)
    {

        // Set up our processInfo to run the git command and log to output and errorOutput.
        ProcessStartInfo processInfo = new ProcessStartInfo("git", @gitCommand)
        {
            CreateNoWindow = true,          // We want no visible pop-ups
            UseShellExecute = false,        // Allows us to redirect input, output and error streams
            RedirectStandardOutput = true,  // Allows us to read the output stream
            RedirectStandardError = true    // Allows us to read the error stream
        };

        // Set up the Process
        Process process = new Process
        {
            StartInfo = processInfo
        };

        try
        {
            process.Start();  // Try to start it, catching any exceptions if it fails
        }
        catch (Exception)
        {
            // For now just assume its failed cause it can't find git.
            Debug.LogError("Git is not set-up correctly, required to be on PATH, and to be a git project.");
            throw;
        }

        // Read the results back from the process so we can get the output and check for errors
        String output = process.StandardOutput.ReadToEnd();
        String errorOutput = process.StandardError.ReadToEnd();

        process.WaitForExit();  // Make sure we wait till the process has fully finished.
        process.Close();        // Close the process ensuring it frees it resources.

        // Check for failure due to no git setup in the project itself or other fatal errors from git.
        if (output.Contains("fatal") || output == "no-git" || output == "")
        {
            throw new Exception("Command: git " + @gitCommand + " Failed\n" + output + errorOutput);
        }
        // Log any errors.
        if (errorOutput != "")
        {
            Debug.LogError("Git Error: " + errorOutput);
        }

        return output;  // Return the output from git.
    }

    public static string RetrieveCurrentCommitShortHash()
    {
        string result = RunGitCommand("rev-parse --short --verify HEAD");
        // Clean up whitespace around hash. (seems to just be the way this command returns :/ )
        result = string.Join("", result.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
        return result;
    }

}
