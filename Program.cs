using System;
using System.Runtime.InteropServices;
using ConsoleApp.Instrumentation;

namespace ConsoleApp
{
    internal class Program
    {
        // ============================
        // Native SDK Imports
        // ============================

        [DllImport("ClassicSdk.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int aw_init(int buildNumber);

        [DllImport("ClassicSdk.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int aw_create(string hostname, int port, out IntPtr instance);

        [DllImport("ClassicSdk.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int aw_login();

        [DllImport("ClassicSdk.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int aw_enter(string world);

        [DllImport("ClassicSdk.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int aw_term();

        [DllImport("ClassicSdk.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int aw_sdk_build();

        [DllImport("ClassicSdk.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int aw_state_change();

        [DllImport("ClassicSdk.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int aw_string(int attribute, string value);

        [DllImport("ClassicSdk.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int aw_string_set(int attribute, string value);

        [DllImport("ClassicSdk.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int aw_int(int attribute);

        [DllImport("ClassicSdk.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int aw_int_set(int attribute, int value);

        [DllImport("ClassicSdk.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int aw_bool_set(int attribute, int value);

        [DllImport("ClassicSdk.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int aw_wait(int milliseconds);

        // ============================
        // Attribute Constants
        // ============================

        const int AW_LOGIN_NAME = 0;
        const int AW_LOGIN_PASSWORD = 1;
        const int AW_LOGIN_OWNER = 2;
        const int AW_LOGIN_PRIVILEGE_PASSWORD = 3;
        const int AW_LOGIN_PRIVILEGE_NUMBER = 4;
        const int AW_LOGIN_PRIVILEGE_NAME = 5;
        const int AW_LOGIN_APPLICATION = 6;
        const int AW_LOGIN_EMAIL = 7;

        const int AW_MY_X = 198;
        const int AW_MY_Y = 199;
        const int AW_MY_Z = 200;
        const int AW_MY_YAW = 201;
        const int AW_MY_PITCH = 202;
        const int AW_MY_TYPE = 203;
        const int AW_MY_GESTURE = 204;
        const int AW_MY_STATE = 205;

        const int AW_ENTER_GLOBAL = 328;

        // ============================
        // Main
        // ============================

        static void Main(string[] args)
        {
            StateTrace.Log("harness_start");

            Console.WriteLine("Yert");


            // aw_init
            StateTrace.Log("aw_init", "build=101");
            int rc = aw_init(101);
            StateTrace.Log("aw_init", "build=101", rc);

            // aw_create
            IntPtr instance;
            StateTrace.Log("aw_create", "host=auth.deltaworlds.com,port=6671");
            rc = aw_create("auth.deltaworlds.com", 6671, out instance);
            StateTrace.Log("aw_create", "host=auth.deltaworlds.com,port=6671", rc);

            if (rc != 0)
            {
                StateTrace.Log("fatal", "aw_create_failed", rc);
                return;
            }

            // Login attributes
            StateTrace.Log("aw_int_set", $"attr={AW_LOGIN_OWNER},value=250");
            aw_int_set(AW_LOGIN_OWNER, 250);

            StateTrace.Log("aw_string_set", $"attr={AW_LOGIN_PRIVILEGE_PASSWORD}");
            aw_string_set(AW_LOGIN_PRIVILEGE_PASSWORD, "455669");

            StateTrace.Log("aw_string_set", $"attr={AW_LOGIN_APPLICATION}");
            aw_string_set(AW_LOGIN_APPLICATION, "SomeTestApp");

            StateTrace.Log("aw_string_set", $"attr={AW_LOGIN_NAME}");
            aw_string_set(AW_LOGIN_NAME, "ThisIsATestBot");

            // aw_login
            StateTrace.Log("aw_login");
            rc = aw_login();
            StateTrace.Log("aw_login", rc: rc);

            if (rc != 0)
            {
                StateTrace.Log("fatal", "aw_login_failed", rc);
                return;
            }

            // Enter world
            StateTrace.Log("aw_bool_set", $"attr={AW_ENTER_GLOBAL},value=0");
            aw_bool_set(AW_ENTER_GLOBAL, 0);

            StateTrace.Log("aw_enter", "world=BotDev");
            rc = aw_enter("BotDev");
            StateTrace.Log("aw_enter", "world=BotDev", rc);

            if (rc != 0)
            {
                StateTrace.Log("fatal", "aw_enter_failed", rc);
                return;
            }

            // Position / avatar state
            StateTrace.Log("aw_int_set", $"attr={AW_MY_X},value=0");
            aw_int_set(AW_MY_X, 0);

            StateTrace.Log("aw_int_set", $"attr={AW_MY_Y},value=0");
            aw_int_set(AW_MY_Y, 0);

            StateTrace.Log("aw_int_set", $"attr={AW_MY_Z},value=0");
            aw_int_set(AW_MY_Z, 0);

            StateTrace.Log("aw_int_set", $"attr={AW_MY_YAW},value=0");
            aw_int_set(AW_MY_YAW, 0);

            StateTrace.Log("aw_int_set", $"attr={AW_MY_TYPE},value=0");
            aw_int_set(AW_MY_TYPE, 0);

            // aw_state_change
            StateTrace.Log("aw_state_change");
            rc = aw_state_change();
            StateTrace.Log("aw_state_change", rc: rc);

            if (rc != 0)
            {
                StateTrace.Log("fatal", "aw_state_change_failed", rc);
                return;
            }

            // Main heartbeat loop
            long waitCount = 0;

            while (true)
            {
                waitCount++;
                rc = aw_wait(100);

                if (waitCount <= 10 || waitCount % 25 == 0)
                {
                    StateTrace.Log("aw_wait", $"iter={waitCount}", rc);
                }
            }
        }
    }
}
