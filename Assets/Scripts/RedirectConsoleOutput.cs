using System.Text;

namespace DefaultNamespace
{
    using System;
    using System.IO;
    using UnityEngine;

    public class RedirectConsoleOutput : MonoBehaviour
    {
        private TextWriter _unityConsoleWriter;

        void Awake()
        {
            // Redirect standard output and error to Unity's Console
            _unityConsoleWriter = new UnityConsoleWriter();
            Console.SetOut(_unityConsoleWriter);
            Console.SetError(_unityConsoleWriter);

            // Test output
            Console.WriteLine("This is a standard output message.");
            Console.Error.WriteLine("This is a standard error message.");
        }

        private void OnDestroy()
        {
            // Restore the default behavior when the script is destroyed
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
            Console.SetError(new StreamWriter(Console.OpenStandardError()) { AutoFlush = true });
        }

        private class UnityConsoleWriter : TextWriter
        {
            public override Encoding Encoding => Encoding.UTF8;

            public override void WriteLine(string value)
            {
                Debug.Log(value);
            }

            public override void Write(string value)
            {
                Debug.Log(value);
            }
        }
    }
}