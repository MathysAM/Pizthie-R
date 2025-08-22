using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using PizthieR.Controller;

namespace PizthieR;

public partial class Programmation : UserControl
{
    public Programmation()
    {
        InitializeComponent();
    }

    private sealed class Day
    {
        public CheckBox Active { get; init; } = default!;
        public ComboBox StartHour { get; init; } = default!;
        public ComboBox StartMinute { get; init; } = default!;
        public ComboBox StopHour { get; init; } = default!;
        public ComboBox StopMinute { get; init; } = default!;
    }

    private readonly List<Day> _days = new();
    private readonly int[] _hours = Enumerable.Range(0, 24).ToArray();
    private readonly int[] _minutes = Enumerable.Range(0, 60).Where(m => m % 5 == 0).ToArray();
 

    MqttController _mqtt;
    public Programmation(MqttController _mqtt)
    {
        InitializeComponent();
        this._mqtt = _mqtt;

        InitDays();

        // Remplit les ComboBox et branche la validation
        for (int i = 0; i < _days.Count; i++)
        {
            var d = _days[i];

            
            // AVANT (erreur CS0200) :
            // d.StartHour.Items   = _hours;

            // APRÈS :
            d.StartHour.ItemsSource = _hours;
            d.StopHour.ItemsSource = _hours;
            d.StartMinute.ItemsSource = _minutes;
            d.StopMinute.ItemsSource = _minutes;

            int idx = i;
            d.StartHour.SelectionChanged += (_, __) => ValidateTime(idx);
            d.StartMinute.SelectionChanged += (_, __) => ValidateTime(idx);
            d.StopHour.SelectionChanged += (_, __) => ValidateTime(idx);
            d.StopMinute.SelectionChanged += (_, __) => ValidateTime(idx);
        }

        // Bouton "Enregistrer" → publish tous les jours
        BtnSaveAll.Click += async (_, __) => await SaveAllAsync();

        
    }

    private void InitDays()
    {
        _days.Clear();
        _days.Add(new Day { Active = Active0, StartHour = StartHour0, StartMinute = StartMinute0, StopHour = StopHour0, StopMinute = StopMinute0 });
        _days.Add(new Day { Active = Active1, StartHour = StartHour1, StartMinute = StartMinute1, StopHour = StopHour1, StopMinute = StopMinute1 });
        _days.Add(new Day { Active = Active2, StartHour = StartHour2, StartMinute = StartMinute2, StopHour = StopHour2, StopMinute = StopMinute2 });
        _days.Add(new Day { Active = Active3, StartHour = StartHour3, StartMinute = StartMinute3, StopHour = StopHour3, StopMinute = StopMinute3 });
        _days.Add(new Day { Active = Active4, StartHour = StartHour4, StartMinute = StartMinute4, StopHour = StopHour4, StopMinute = StopMinute4 });
        _days.Add(new Day { Active = Active5, StartHour = StartHour5, StartMinute = StartMinute5, StopHour = StopHour5, StopMinute = StopMinute5 });
        _days.Add(new Day { Active = Active6, StartHour = StartHour6, StartMinute = StartMinute6, StopHour = StopHour6, StopMinute = StopMinute6 });
    }

    // Abonnements MQTT (mêmes topics que ta page Blazor)
    public async Task SubscribeAllAsync()
    {
        for (int i = 0; i < _days.Count; i++)
        {
            int idx = i;

            await _mqtt.SubscribeAsync($"/Prog/{idx}/Active", async (payload, _) =>
            {
                bool val = ParseStrictBool(payload);
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    _days[idx].Active.IsThreeState = false;
                    _days[idx].Active.IsChecked = val;
                  
                });
            }, 0);

            await _mqtt.SubscribeAsync($"/Prog/{idx}/StartHour", async (payload, _) =>
            {
                if (int.TryParse(payload, out var v))
                    await UI(() => SelectValue(_days[idx].StartHour, Clamp(v, 0, 23)));
            }, 0);

            await _mqtt.SubscribeAsync($"/Prog/{idx}/StartMinute", async (payload, _) =>
            {
                if (int.TryParse(payload, out var v))
                    await UI(() => SelectValue(_days[idx].StartMinute, ClampTo5(v)));
            }, 0);

            await _mqtt.SubscribeAsync($"/Prog/{idx}/StopHour", async (payload, _) =>
            {
                if (int.TryParse(payload, out var v))
                    await UI(() => SelectValue(_days[idx].StopHour, Clamp(v, 0, 23)));
            }, 0);

            await _mqtt.SubscribeAsync($"/Prog/{idx}/StopMinute", async (payload, _) =>
            {
                if (int.TryParse(payload, out var v))
                    await UI(() => SelectValue(_days[idx].StopMinute, ClampTo5(v)));
            }, 0);
        }
    }

    private static bool ParseStrictBool(string? s)
    {
        if (s is null) return false;
        s = s.Trim();                 // retire espaces/CRLF
        return string.Equals(s, "true", StringComparison.OrdinalIgnoreCase);
        // "false" → false ; "0"/"on"/"Active = false" → false ; etc.
    }

    public async Task UnsubscribeAllAsync()
    {
        for (int i = 0; i < _days.Count; i++)
        {
            await _mqtt.UnsubscribeAsync($"/Prog/{i}/Active");
            await _mqtt.UnsubscribeAsync($"/Prog/{i}/StartHour");
            await _mqtt.UnsubscribeAsync($"/Prog/{i}/StartMinute");
            await _mqtt.UnsubscribeAsync($"/Prog/{i}/StopHour");
            await _mqtt.UnsubscribeAsync($"/Prog/{i}/StopMinute");
        }
    }

    private async Task SaveAllAsync()
    {
        for (int i = 0; i < _days.Count; i++)
        {
            var d = _days[i];
            bool isActive = d.Active.IsChecked == true;

            int sh = GetSelectedInt(d.StartHour);
            int sm = GetSelectedInt(d.StartMinute);
            int eh = GetSelectedInt(d.StopHour);
            int em = GetSelectedInt(d.StopMinute);

            await _mqtt.PublishActive($"/Prog/{i}/Active", isActive ? "true" : "false");
            await _mqtt.PublishActive($"/Prog/{i}/StartHour", sh.ToString());
            await _mqtt.PublishActive($"/Prog/{i}/StartMinute", sm.ToString());
            await _mqtt.PublishActive($"/Prog/{i}/StopHour", eh.ToString());
            await _mqtt.PublishActive($"/Prog/{i}/StopMinute", em.ToString());
        }
    }

    // === Validation : durée ≥ 60 min, roulage minuit ===
    private void ValidateTime(int index)
    {
        var d = _days[index];

        int start = GetSelectedInt(d.StartHour) * 60 + GetSelectedInt(d.StartMinute);
        int stop = GetSelectedInt(d.StopHour) * 60 + GetSelectedInt(d.StopMinute);

        if (stop - start < 60)
        {
            if (stop < start)
            {
                // recule start vers stop (roulage minuit)
                int newH = (GetSelectedInt(d.StopHour) + 23) % 24;
                int newM = GetSelectedInt(d.StopMinute);
                SelectValue(d.StartHour, newH);
                SelectValue(d.StartMinute, newM);
            }
            else
            {
                // pousse stop vers start
                int newH = (GetSelectedInt(d.StartHour) + 1) % 24;
                int newM = GetSelectedInt(d.StartMinute);
                SelectValue(d.StopHour, newH);
                SelectValue(d.StopMinute, newM);
            }
        }
    }

    // === Helpers UI/valeurs ===
    private static Task UI(Action a) => Dispatcher.UIThread.InvokeAsync(a).GetTask();

    private static void SelectValue(ComboBox combo, int value)
    {
        if (combo.ItemsSource is IEnumerable<int> data)
        {
            int idx = 0;
            int found = -1;
            foreach (var v in data)
            {
                if (v == value) { found = idx; break; }
                idx++;
            }
            combo.SelectedIndex = found >= 0 ? found : 0;
        }
    }

    private static int GetSelectedInt(ComboBox combo)
    {
        if (combo.SelectedItem is int v) return v;

        if (combo.ItemsSource is IEnumerable<int> data)
        {
            // si rien de sélectionné, on prend le 1er
            var e = data.GetEnumerator();
            return e.MoveNext() ? e.Current : 0;
        }
        return 0;
    }

    private static int Clamp(int v, int min, int max) => v < min ? min : (v > max ? max : v);

    private static int ClampTo5(int v)
    {
        v = Clamp(v, 0, 59);
        return v - (v % 5); // 0,5,10,...,55
    }
}
