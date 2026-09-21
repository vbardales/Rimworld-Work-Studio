using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace WorkStudio.PickleSteps
{
    /// <summary>
    /// Remembers the rectangles of the image buttons drawn during the last two frames, so a click
    /// that reached a button and opened nothing can say which control sat on the point.
    /// <para>
    /// Pickle records only <c>Widgets.ButtonText</c>, under a <c>btn:</c> tag. A button with no label -
    /// Work Tab's three 30x30 toggles are <c>Widgets.ButtonImage</c> - is invisible to it, which is
    /// exactly why a click taken by one of them left a report with no reason in it. This is the
    /// local measure for that hole; it changes nothing in Pickle, and it only exists while the
    /// tests run.
    /// </para>
    /// <para>
    /// Rectangles are converted the way Pickle converts its own (<c>GUIUtility.GUIToScreenRect</c>,
    /// during <c>Repaint</c> only), so a point taken from a tag and a rectangle taken from here are
    /// in the same space.
    /// </para>
    /// </summary>
    internal static class ImageButtonProbe
    {
        internal struct Seen
        {
            public Rect Screen;
            public string Widget;
            public string Label;
            public int Order;
        }

        private static readonly string[] Widgets_ = { "ButtonImage", "ButtonImageFitted", "ButtonImageWithBG", "ButtonImageDraggable" };

        private static bool installed;
        private static int frame = -1;
        private static int order;
        private static List<Seen> current = new List<Seen>();
        private static List<Seen> previous = new List<Seen>();

        /// <summary>Idempotent: the patch goes on once per game, however many scenarios ask.</summary>
        internal static void EnsureInstalled()
        {
            if (installed)
            {
                return;
            }

            installed = true;
            var harmony = new Harmony("workstudio.pickletests.imagebuttonprobe");
            var postfix = new HarmonyMethod(typeof(ImageButtonProbe), nameof(Postfix));

            // Every overload: they are separate methods and a control may call any of them. All of
            // them take the rectangle first, which is what the postfix reads by position.
            foreach (var method in typeof(Widgets)
                         .GetMethods(BindingFlags.Public | BindingFlags.Static)
                         .Where(m => Widgets_.Contains(m.Name)))
            {
                harmony.Patch(method, postfix: postfix);
            }

            // ButtonText too, with its label: Pickle tags these, but does not say WHERE it recorded
            // them, and a click that lands somewhere other than the drawn button needs that number.
            // Arguments are read as an array because the overloads do not agree on the label's type.
            var textPostfix = new HarmonyMethod(typeof(ImageButtonProbe), nameof(TextPostfix));
            foreach (var method in typeof(Widgets)
                         .GetMethods(BindingFlags.Public | BindingFlags.Static)
                         .Where(m => m.Name == "ButtonText"))
            {
                harmony.Patch(method, postfix: textPostfix);
            }
        }

        public static void TextPostfix(object[] __args, MethodBase __originalMethod)
        {
            if (__args == null || __args.Length < 2 || !(__args[0] is Rect rect))
            {
                return;
            }

            Record(rect, "ButtonText", __args[1]?.ToString());
        }

        public static void Postfix(Rect __0, MethodBase __originalMethod)
        {
            Record(__0, __originalMethod.Name, null);
        }

        private static void Record(Rect rect, string widget, string label)
        {
            if (Event.current == null || Event.current.type != EventType.Repaint)
            {
                return;
            }

            if (Time.frameCount != frame)
            {
                previous = current;
                current = new List<Seen>();
                frame = Time.frameCount;
                order = 0;
            }

            current.Add(new Seen
            {
                Screen = GUIUtility.GUIToScreenRect(rect),
                Widget = widget,
                Label = label,
                Order = order++,
            });
        }

        /// <summary>
        /// Where a text button with this label was drawn, in the space Pickle records its tags in.
        /// The click lands at the centre of the rectangle Pickle stored, so when the pointer is
        /// nowhere near the button on the screenshot, this is the number that says whether the
        /// rectangle or the pointer is the one that is wrong.
        /// </summary>
        internal static string DescribeText(string label)
        {
            var matches = previous.Concat(current)
                .Where(seen => seen.Widget == "ButtonText" && seen.Label == label)
                .GroupBy(seen => seen.Screen)
                .Select(group => group.First())
                .ToList();

            if (matches.Count == 0)
            {
                return $"  no Widgets.ButtonText labelled '{label}' was seen in the last two frames";
            }

            return string.Join("\n", matches.Select(s =>
                $"  Widgets.ButtonText '{label}' drawn at {s.Screen}, centre {s.Screen.center}"));
        }

        /// <summary>
        /// The image buttons whose rectangle contains a UI-space point, in the order they were drawn.
        /// Two frames are looked at because the click step reads a frame or two after the button was
        /// drawn, and a rectangle drawn on both is reported once.
        /// </summary>
        internal static List<Seen> At(Vector2 uiPoint)
        {
            var screen = uiPoint * Prefs.UIScale;

            return previous.Concat(current)
                .Where(seen => seen.Screen.Contains(screen))
                .GroupBy(seen => seen.Screen)
                .Select(group => group.First())
                .OrderBy(seen => seen.Order)
                .ToList();
        }

        /// <summary>The lines a failure message shows for them, or a sentence saying there were none.</summary>
        internal static string Describe(Vector2 uiPoint)
        {
            var seen = At(uiPoint);
            if (seen.Count == 0)
            {
                return "  no image button contains the pointer (Widgets.ButtonImage and its variants)";
            }

            return string.Join("\n", seen.Select(s =>
                $"  Widgets.{s.Widget} at {s.Screen}, drawn #{s.Order} this frame"));
        }
    }
}
