using System.Reflection;
using System.Runtime.ExceptionServices;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.FluentUI.AspNetCore.Components;
using NetTrace;

// These native tests deliberately inspect generated render-tree frames, not Razor source.
#pragma warning disable BL0006

namespace DotnetEventsViewer.Test;

internal static class ComponentTestAccess
{
    internal static void SetProperty(object instance, string name, object value) =>
        instance.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
            .SetValue(instance, value);

    internal static T Invoke<T>(object instance, string name, params object[] arguments)
    {
        var method = instance.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)!;
        try
        {
            return (T)method.Invoke(instance, arguments)!;
        }
        catch (TargetInvocationException exception) when (exception.InnerException != null)
        {
            ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
            throw;
        }
    }

    internal static RenderedComponent[] Render(ComponentBase component)
    {
        using RenderTreeBuilder builder = new();
        Invoke<object?>(component, "BuildRenderTree", builder);
        return ReadComponents(builder);
    }

    internal static RenderedComponent[] Render(RenderFragment fragment)
    {
        using RenderTreeBuilder builder = new();
        fragment(builder);
        return ReadComponents(builder);
    }

    private static RenderedComponent[] ReadComponents(RenderTreeBuilder builder)
    {
        var frames = builder.GetFrames();
        List<RenderedComponent> components = [];
        for (int i = 0; i < frames.Count; i++)
        {
            var frame = frames.Array[i];
            if (frame.FrameType != RenderTreeFrameType.Component) continue;
            Dictionary<string, object?> parameters = [];
            Action<object>? capture = null;
            for (int j = i + 1; j < i + frame.ComponentSubtreeLength; j++)
            {
                var child = frames.Array[j];
                if (child.FrameType == RenderTreeFrameType.Attribute)
                    parameters.Add(child.AttributeName, child.AttributeValue);
                else if (child.FrameType == RenderTreeFrameType.ComponentReferenceCapture)
                    capture = child.ComponentReferenceCaptureAction;
            }
            components.Add(new(frame.ComponentType, frame.ComponentKey, parameters, capture));
            i += frame.ComponentSubtreeLength - 1;
        }
        return components.ToArray();
    }

    internal sealed record RenderedComponent(Type Type, object? Key,
        IReadOnlyDictionary<string, object?> Parameters, Action<object>? Capture)
    {
        internal T Parameter<T>(string name) => (T)Parameters[name]!;
    }

    // Only supply a render handle for Fluent's real sort methods. No component initialization,
    // DOM, JS interop, or viewport is involved; the page's own generated tree is inspected above.
    private sealed class NoServices : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    internal sealed class SortRenderer() : Renderer(new NoServices(), NullLoggerFactory.Instance)
    {
        public override Dispatcher Dispatcher { get; } = Dispatcher.CreateDefault();
        internal Task AttachAsync(IComponent component) => Dispatcher.InvokeAsync(() => { AssignRootComponentId(component); });
        protected override Task UpdateDisplayAsync(in RenderBatch renderBatch) => Task.CompletedTask;
        protected override void HandleException(Exception exception) => ExceptionDispatchInfo.Capture(exception).Throw();
    }

    internal sealed class SortGrid() : FluentDataGrid<Event>(new LibraryConfiguration())
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder) { }
        protected override Task OnAfterRenderAsync(bool firstRender) => Task.CompletedTask;
    }
}
