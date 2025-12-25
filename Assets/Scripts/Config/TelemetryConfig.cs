using UnityEngine;

namespace SyntheticLife.Phi.Config
{
    [CreateAssetMenu(fileName = "TelemetryConfig", menuName = "SyntheticLife/Config/TelemetryConfig")]
    public class TelemetryConfig : ScriptableObject
    {
        [Header("Telemetry Settings")]
        public bool enableTelemetry = true;
        public bool logToFile = true;

        [Header("Episodes Logging")]
        public bool logEpisodes = true;

        [Header("Events Logging")]
        public bool logEvents = true;
        public int eventsBufferSize = 100;

        [Header("Trajectory Snapshots")]
        public bool logTrajectories = true;
        public int trajectorySampleInterval = 10; // Every N steps

        [Header("World Snapshots")]
        public bool logWorldSnapshots = true;
        public float worldSnapshotInterval = 30f; // Every N seconds
        public int phiGridResolution = 64;

        [Header("Replay Artifacts")]
        public bool logReplayData = true;

        [Header("Reporting")]
        public bool generateAutoReport = true;
    }
}

