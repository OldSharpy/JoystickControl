using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace JoystickControl;

/// <summary>
/// Joystick control
/// </summary>
public class Joystick : ContentView
{
    bool isTouching;
    SKCanvasView canvasView;
    SKPoint center = SKPoint.Empty;
    SKPoint thumbPosition;
    float directionX, directionY, radius, thumbRadius;


    public event EventHandler<JoystickEventArgs>? JoystickMoved;

    public Joystick()
    {
        canvasView = new SKCanvasView() { EnableTouchEvents = true };

        canvasView.PaintSurface += OnCanvasViewPaintSurface;
        canvasView.Touch += OnCanvasViewTouch;

        Content = canvasView;
        Content.SizeChanged += OnContentSizeChanged;

        Task.Run(KeepMoving);

    }

    #region _- Command -_

    // Declare the Command property
    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command),
        typeof(ICommand),
        typeof(Joystick),
        null);

    // Declare the CommandParameter property
    public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(
        nameof(CommandParameter),
        typeof(object),
        typeof(Joystick),
        null);

    /// <summary>
    /// Command to execute when the joystick is moved
    /// </summary>
    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>
    /// Command parameter to pass when the joystick is moved
    /// </summary>
    public object CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }


    #endregion _- Command -_

    #region _- Main bindable color -_

    public static readonly BindableProperty MainGradientStartColorProperty = BindableProperty.Create(
        nameof(MainGradientStartColor),
        typeof(Color),
        typeof(Joystick),
        Colors.WhiteSmoke.WithAlpha(0.50f));

    public static readonly BindableProperty MainGradientEndColorProperty = BindableProperty.Create(
        nameof(MainGradientEndColor),
        typeof(Color),
        typeof(Joystick),
        Colors.WhiteSmoke);

    public Color MainGradientStartColor
    {
        get => (Color)GetValue(MainGradientStartColorProperty);            
        set => SetValue(MainGradientStartColorProperty, value);
    }

    public Color MainGradientEndColor
    {
        get => (Color)GetValue(MainGradientEndColorProperty);
        set => SetValue(MainGradientEndColorProperty, value);
    }

    #endregion _- Main bindable color -_

    #region _- Border bindable color -_

    public static readonly BindableProperty BorderColorProperty = BindableProperty.Create(
        nameof(BorderColor),
        typeof(Color),
        typeof(Joystick),
        Colors.WhiteSmoke);

    public Color BorderColor
    {
        get => (Color)GetValue(BorderColorProperty);
        set => SetValue(BorderColorProperty, value);
    }

    #endregion _- Border bindable color -_

    #region _- Border bindable thickness -_

    public static readonly BindableProperty BorderThicknessProperty = BindableProperty.Create(
        nameof(BorderThickness),
        typeof(float),
        typeof(Joystick),
        1.0f);

    public float BorderThickness
    {
        get => (float)GetValue(BorderThicknessProperty);
        set => SetValue(BorderThicknessProperty, value);
    }

    #endregion

    #region _- Thumb bindable color -_

    public static readonly BindableProperty ThumbGradientStartColorProperty = BindableProperty.Create(
    nameof(ThumbGradientStartColor),
    typeof(Color),
    typeof(Joystick),
    Colors.WhiteSmoke.WithAlpha(0.5f));

    public static readonly BindableProperty ThumbGradientEndColorProperty = BindableProperty.Create(
        nameof(ThumbGradientEndColor),
        typeof(Color),
        typeof(Joystick),
        Colors.WhiteSmoke);

    public Color ThumbGradientStartColor
    {
        get => (Color)GetValue(ThumbGradientStartColorProperty);
        set => SetValue(ThumbGradientStartColorProperty, value);
    }

    public Color ThumbGradientEndColor
    {
        get => (Color)GetValue(ThumbGradientEndColorProperty);
        set => SetValue(ThumbGradientEndColorProperty, value);
    }

    #endregion _- Thumb bindable color -_

    #region _- Thumb bindable radius -_


    public static readonly BindableProperty ThumbRadiusProperty = BindableProperty.Create(
        nameof(ThumbRadius),
        typeof(float?),
        typeof(Joystick),
        null);

    public float? ThumbRadius
    {
        get => (float?)GetValue(ThumbRadiusProperty);
        set => SetValue(ThumbRadiusProperty, value);
    }

    #endregion _- Thumb bindable radius -_

    private void OnContentSizeChanged(object? sender, EventArgs e)
    {
        center = SKPoint.Empty;
    }

    private void OnCanvasViewTouch(object? sender, SKTouchEventArgs e)
    {
        // check if the touch action is pressed or moved
        if (e.ActionType == SKTouchAction.Pressed || e.ActionType == SKTouchAction.Moved)
        {
            isTouching = true;
            // calculate the direction from the center of the joystick to the touch point
            var touchPoint = e.Location;
            var direction = touchPoint - center;

            // distance to keep the thumb circle inside the bigger circle
            var adjustedRadius = radius - thumbRadius;
            var maxDistance = new SKPoint(0.0f, adjustedRadius);


            if (direction.Length > maxDistance.Length)
            {
                // Normalize the direction vector and scale it to ensure the thumb stays within the joystick's radius
                var normalizedDirection = SKPoint.Normalize(direction);
                var scaledDirection = new SKPoint(normalizedDirection.X * maxDistance.Length, normalizedDirection.Y * maxDistance.Length);
                direction = scaledDirection;
            }

            // update thumb position to the new calculated position
            thumbPosition = center + direction;
            canvasView.InvalidateSurface();

            directionX = direction.X / adjustedRadius;
            directionY = direction.Y / adjustedRadius;
            //Console.WriteLine($"{directionX} x {directionY}");
            OnJoystickMoved(new JoystickEventArgs() { X = directionX, Y = directionY });

            e.Handled = true;
        }
        else if (e.ActionType == SKTouchAction.Released)
        {
            isTouching = false;
            thumbPosition = center;
            canvasView.InvalidateSurface();
            directionX = 0;
            directionY = 0;
            OnJoystickMoved(new JoystickEventArgs() { X = directionX, Y = directionY });
            e.Handled = true;
        }
    }

    private void OnCanvasViewPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear();

        var scale = e.Info.Width / canvasView.Width;
        var scaledWidth = (float)(canvasView.Width * scale);
        var scaledHeight = (float)(canvasView.Height * scale);
        if (center.IsEmpty)
        {
            center = new SKPoint(scaledWidth / 2, scaledHeight / 2);
            thumbPosition = center;
        }

        radius = (float)Math.Min(scaledWidth - this.Padding.HorizontalThickness, scaledHeight - this.Padding.VerticalThickness) / 2;

        var startColor = MainGradientStartColor.ToSKColor();
        var endColor = MainGradientEndColor.ToSKColor();

        // Debug logging
        Console.WriteLine($"Start Color: {startColor}");
        Console.WriteLine($"End Color: {endColor}");

        var gradientPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,

            Shader = SKShader.CreateRadialGradient(
                center, radius,
                new SKColor[] { MainGradientStartColor.ToSKColor(), MainGradientEndColor.ToSKColor() },
                new float[] { 0.0f, 1.0f },
                SKShaderTileMode.Clamp)
        };

        // Draw the background circle
        var borderPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = BorderColor.ToSKColor(),
            StrokeWidth = BorderThickness
        };

        canvas.DrawCircle(center, radius, gradientPaint);
        canvas.DrawCircle(center, radius, borderPaint);

        // Draw the thumb circle
        thumbRadius = (ThumbRadius == null || ThumbRadius == float.NaN) ? radius / 4 : ThumbRadius.Value;
        var thumbPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Shader = SKShader.CreateRadialGradient(
                center, thumbRadius,
                new SKColor[] { ThumbGradientStartColor.ToSKColor(), ThumbGradientEndColor.ToSKColor() },
                new float[] { 0.0f, 1.0f },
                SKShaderTileMode.Clamp
                )
        };
        canvas.DrawCircle(thumbPosition, thumbRadius, gradientPaint);

    }

    private async Task KeepMoving()
    {
        while (true)
        {
            if (isTouching)
            {
                OnJoystickMoved(new JoystickEventArgs() { X = directionX, Y = directionY });
            }
            await Task.Delay(3);
        }

    }

    protected virtual void OnJoystickMoved(JoystickEventArgs e)
    {
        JoystickMoved?.Invoke(this, e);
        CommandParameter = e;

        if (Command != null && Command.CanExecute(CommandParameter))
        {
            Command.Execute(CommandParameter);
        }
    }

    protected override void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        base.OnPropertyChanged(propertyName);

        if ( propertyName == nameof(MainGradientStartColor)
            || propertyName == nameof(MainGradientEndColor)
            || propertyName == nameof(BorderColor)
            || propertyName == nameof(ThumbGradientStartColor)
            || propertyName == nameof(ThumbGradientEndColor))
        {
            canvasView.InvalidateSurface();
        }
    }








}