// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Nour.Play.Areas.Evolving {
    internal class ForceFormula : IForceFormula {
        private const double K = 1;

        /// <summary>
        /// Gets force between colliding objects. 
        /// </summary>
        /// <param name="sign">Indicates the collision sign.</param>
        /// <param name="fragment">Fragment size to boost time.</param>
        /// <returns></returns>
        public double CollideForce(double sign, double fragment) => K / fragment * Math.Sign(sign);

        public double NormalForce(double distance) {
            var sign = Math.Sign(distance);
            // cap distance between 0 and 10
            distance = Math.Min(Math.Abs(distance), 10);
            // the force is capped between 0 and 3
            var force = (3.3 / (distance + 1)) - 0.3;
            return force * sign;
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Gets force between overlapping objects. 
        /// </summary>
        /// <param name="distance">Indicates how much the objects overlap in system units.</param>
        /// <param name="fragment">Fragment size to boost time.</param>
        public double OverlapForce(double distance, double fragment) => (distance + Math.Sign(distance)) / fragment;
    }
}