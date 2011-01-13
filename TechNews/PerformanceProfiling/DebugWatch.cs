namespace TechNews.PerformanceProfiling
{
    /// A simple profiling tool. Represents single instance of  the standard StopWatch class.
    /// Remarks: All methods are compiled conditionally in the  DEBUG mode.
    public class DebugWatch{
        static  private System.Diagnostics.Stopwatch _mWatch;
                
        static  private void AssertValid()  {
            if  (_mWatch == null)
                _mWatch  = new System.Diagnostics.Stopwatch();
        }
                
        ///  <summary>Resets time measurement.</summary>

        [System.Diagnostics.Conditional("DEBUG")]
        static  public void Reset()  {
            _mWatch  = null;
        }
                
        ///  <summary>Starts time measurement.</summary>

        [System.Diagnostics.Conditional("DEBUG")]
        static  public void Start()  {
            AssertValid();
            _mWatch.Start();
        }
                
        ///  <summary>Stops time measurement</summary>

        [System.Diagnostics.Conditional("DEBUG")]
        static  public void Stop()  {
            AssertValid();
            _mWatch.Stop();
        }
                
        ///  <summary>Outputs the specified prompt followed by " n  msec".</summary>

        ///  <param name="prompt">The prompt.</param>
        [System.Diagnostics.Conditional("DEBUG")]
        static  public void Print(string prompt)  {
            AssertValid();
            System.Diagnostics.Debug.WriteLine("{0}  {1} msec", 
                                               prompt, _mWatch.ElapsedMilliseconds);
        }
    }
}
