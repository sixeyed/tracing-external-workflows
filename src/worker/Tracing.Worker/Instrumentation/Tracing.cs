
using System.Diagnostics;

namespace Tracing.Worker.Instrumentation;

public static class Tracing
{
    public const string NAME = "sample-tracing";
    public static readonly ActivitySource ActivitySource = new(NAME);
}