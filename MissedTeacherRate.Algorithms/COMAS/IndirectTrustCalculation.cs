using MissedTeacherRate.Algorithms.COMAS.Models;
using MissedTeacherRate.Models;

namespace ComputeTrust.Algorithms
{
    internal static class IndirectTrustCalculation
    {
        public static void FillIndirectTrusts(
            List<Trust<Teacher, Student>> teacherStudentTrusts,
            List<Trust<Student, Student>> studentStudentTrusts)
        {
            // Build the graph: nodes are Person, edges are trust.Value (if not null)
            var nodes = new HashSet<Person>();
            var edges = new Dictionary<Person, List<(Person, double)>>();

            // Add teacher-student direct trusts
            foreach (var trust in teacherStudentTrusts)
            {
                nodes.Add(trust.First);
                nodes.Add(trust.Second);
                if (trust.Value != null)
                {
                    AddEdge(edges, trust.First, trust.Second, trust.Value.Value);
                    AddEdge(edges, trust.Second, trust.First, trust.Value.Value);
                }
            }

            // Add student-student direct trusts
            foreach (var trust in studentStudentTrusts)
            {
                nodes.Add(trust.First);
                nodes.Add(trust.Second);
                if (trust.Value != null)
                {
                    AddEdge(edges, trust.First, trust.Second, trust.Value.Value);
                    AddEdge(edges, trust.Second, trust.First, trust.Value.Value);
                }
            }

            // For each missing teacher-student trust, compute indirect trust
            foreach (var trust in teacherStudentTrusts.Where(t => t.Value == null))
            {
                var indirectTrust = FindIndirectTrust(trust.First, trust.Second, nodes, edges);
                trust.Type = TrustType.Indirect;
                trust.Value = indirectTrust;
            }
        }

        // Helper to add an edge to the adjacency list
        private static void AddEdge(Dictionary<Person, List<(Person, double)>> edges, Person from, Person to, double trustValue)
        {
            if (!edges.ContainsKey(from))
                edges[from] = new List<(Person, double)>();
            edges[from].Add((to, trustValue));
        }

        // Dijkstra's algorithm to find the path with the highest product of trust values
        private static double? FindIndirectTrust(Person start, Person end, HashSet<Person> nodes, Dictionary<Person, List<(Person, double)>> edges)
        {
            // Use negative log to convert product maximization to sum minimization
            var dist = new Dictionary<Person, double>();
            var prev = new Dictionary<Person, Person?>();
            var queue = new PriorityQueue<Person, double>();

            foreach (var node in nodes)
                dist[node] = double.PositiveInfinity;
            dist[start] = 0;
            queue.Enqueue(start, 0);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (current.Equals(end))
                    break;

                if (!edges.ContainsKey(current))
                    continue;

                foreach (var (neighbor, trustValue) in edges[current])
                {
                    // Avoid log(0) or negative trust
                    if (trustValue <= 0) continue;
                    double weight = -Math.Log(trustValue); // negative log for product maximization

                    double alt = dist[current] + weight;
                    if (alt < dist[neighbor])
                    {
                        dist[neighbor] = alt;
                        prev[neighbor] = current;
                        queue.Enqueue(neighbor, alt);
                    }
                }
            }

            if (double.IsInfinity(dist[end]))
                return null; // No path

            // Reconstruct path and calculate product of trust values
            var path = new List<Person>();
            for (var at = end; prev.ContainsKey(at); at = prev[at]!)
                path.Add(at);
            path.Add(start);
            path.Reverse();

            // If path is only start, no indirect trust
            if (path.Count < 2)
                return null;

            double product = 1.0;
            for (int i = 0; i < path.Count - 1; i++)
            {
                var from = path[i];
                var to = path[i + 1];
                var edgeTrust = edges[from].First(e => e.Item1.Equals(to)).Item2;
                product *= edgeTrust;
            }
            return product;
        }
    }
}
