using System.Diagnostics;
using System.Text;

namespace GitWorkTreeInit
{
   internal class Program
   {
      static void Main(string[] args)
      {
         if (args.Length == 0)
         {
            Console.Error.WriteLine("Needs Folder");
            return;
         }
         string path = args[0];
         if (!Path.IsPathFullyQualified(path))
         {
            Console.Error.WriteLine($"{path} is not a path");
            return;
         }

         var branches = GetBranches(path);
         if (branches.Length > 0)
         {
            if (args.Length > 1)
            {
               if (args[1] == "-r")
               {
                  Remove(path, branches);
               }
            }
            else
               InitWorktrees(path, branches);
         }

      }

      private static void InitWorktrees(string path, string[] branches)
      {
         var proc = new Process();

         var psi = new ProcessStartInfo("cmd.exe");

         psi.UseShellExecute = false;
         psi.RedirectStandardOutput = true;
         psi.RedirectStandardError = true;
         psi.RedirectStandardInput = true;
         psi.WorkingDirectory = path;

         proc.StartInfo = psi;
         proc.Start();
         proc.OutputDataReceived += Proc_OutputDataReceived;
         proc.ErrorDataReceived += Proc_OutputDataReceived;

         proc.BeginOutputReadLine();
         proc.BeginErrorReadLine();

         StreamWriter sw = proc.StandardInput;

         foreach (var branch in branches)
         {
            sw.WriteLine($"git worktree add ../{branch} svn/{branch}");
         }
         sw.Close();
         proc.WaitForExit();


      }

      private static void Remove(string path, string[] branches)
      {
         var proc = new Process();

         var psi = new ProcessStartInfo("cmd.exe");

         psi.UseShellExecute = false;
         psi.RedirectStandardOutput = true;
         psi.RedirectStandardError = true;
         psi.RedirectStandardInput = true;
         psi.WorkingDirectory = path;

         proc.StartInfo = psi;
         proc.Start();
         proc.OutputDataReceived += Proc_OutputDataReceived;
         proc.ErrorDataReceived += Proc_OutputDataReceived;

         proc.BeginOutputReadLine();
         proc.BeginErrorReadLine();

         StreamWriter sw = proc.StandardInput;

         foreach (var branch in branches)
         {
            sw.WriteLine($"git worktree remove {branch}");
         }
         sw.Close();
         proc.WaitForExit();




      }

      private static string[] GetBranches(string path)
      {
         var proc = new Process();

         var psi = new ProcessStartInfo("cmd.exe");

         psi.UseShellExecute = false;
         psi.RedirectStandardOutput = true;
         psi.RedirectStandardError = true;
         psi.RedirectStandardInput = true;
         psi.CreateNoWindow = true;
         psi.WorkingDirectory = path;

         proc.StartInfo = psi;
         proc.Start();
         proc.OutputDataReceived += Proc_OutputDataReceived;
         proc.OutputDataReceived += WriteOutputToBuilder;
         proc.ErrorDataReceived += Proc_OutputDataReceived;
         proc.BeginErrorReadLine();
         proc.BeginOutputReadLine();

         using (StreamWriter sw = proc.StandardInput)
         {
            sw.AutoFlush = true;
            sw.WriteLine("git branch -r");
            sw.Close();
         }

         proc.WaitForExit();
         proc.OutputDataReceived -= Proc_OutputDataReceived;
         proc.OutputDataReceived -= WriteOutputToBuilder;
         proc.ErrorDataReceived -= Proc_OutputDataReceived;
         proc.Close();
         proc.Dispose();

         var res = sb.ToString().Split(Environment.NewLine).Where(x => x.Contains("svn/")).Select(x => x.Replace("origin/svn/", "").Trim()).ToArray();

         sb.Clear();
         return res;
      }

      private static StringBuilder sb = new();
      private static void WriteOutputToBuilder(object sender, DataReceivedEventArgs e)
      {
         sb.AppendLine(e.Data);
      }

      private static void Proc_OutputDataReceived(object sender, DataReceivedEventArgs e)
      {
         Console.WriteLine(e.Data);
      }
   }
}