using System;
using System.Collections.Generic;
using System.Windows.Forms;

using BoogieBot.Common;


namespace BoogieBot.GUIApp
{
    class Program
    {
        public static BoogieBot bot;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            bot = new BoogieBot();

            try
            {
                BoogieCore.InitCore(Log, EventHandler);
                Application.Run(bot);
            }
            catch (Exception ex)
            {
                String error = String.Format("Error: {0}\n\nStackTrace:\n\n{1}", ex.InnerException.Message, ex.InnerException.StackTrace);
                MessageBox.Show(error, ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        delegate void EventInvoke(Event e);
        delegate void LogInvoke(LogType lt, string text, params object[] parameters);

        // Event Handler
        public static void EventHandler(Event e)
        {
            if (bot.InvokeRequired)
               bot.Invoke(new EventInvoke(EventHandler), new object[] { e });
            else
                bot.EventHandler(e);
        }

        // Log Handler
        public static void Log(LogType lt, string format, params object[] parameters)
        {
            if (bot.InvokeRequired)
                bot.Invoke(new LogInvoke(Log), new object[] { lt, format, parameters });
            else
                bot.Log(lt, String.Format(format, parameters));
        }
    }
}