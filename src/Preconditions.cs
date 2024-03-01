
using System;

namespace PlayersWorlds.Maps {
    public static class Preconditions {
        /// <summary>
        /// Checks a precondition and throws an ArgumentException if the check
        /// fails.
        /// </summary>
        /// <param name="condition">The condition to check.</param>
        /// <param name="message">The message to include in the exception, if necessary.</param>
        public static void Check(bool condition, string message) =>
            Check<ArgumentException>(condition, message);

        /// <summary>
        /// Checks a precondition and throws an exception if the check fails.
        /// </summary>
        /// <typeparam name="T">The type of the exception to throw.</typeparam>
        /// <param name="condition">The condition to check.</param>
        /// <param name="message">The message to include in the exception, if necessary.</param>
        public static void Check<T>(bool condition, string message)
              where T : Exception {
            if (!condition)
                throw (T)Activator.CreateInstance(typeof(T), message);
        }
    }
}