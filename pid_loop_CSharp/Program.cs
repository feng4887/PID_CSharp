namespace pid_loop_CSharp
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            string samplingPath = Path.Combine(AppContext.BaseDirectory, "pid_sampling.cfg");
            PidSamplingSettings samplingSettings;
            try
            {
                samplingSettings = PidSamplingSettings.Load(samplingPath);
            }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException
                or FormatException or ArgumentException)
            {
                MessageBox.Show($"Cannot load sampling settings:\n{samplingPath}\n\n{error.Message}",
                    "Sampling configuration error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Application.Run(new frmMain(samplingSettings));
        }
    }
}
