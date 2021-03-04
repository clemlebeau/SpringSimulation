using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;

namespace SpringSimulation
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private const float DEFAULT_K = .01f;

        bool _pauseSimulation = false;
        bool _applyGravity = true;

        PictureBox pictureBox;
        private void MainForm_Load(object sender, EventArgs e)
        {
            this.FormBorderStyle = FormBorderStyle.None;

            this.TopMost = true;
            this.Bounds = Screen.PrimaryScreen.Bounds;
            //this.Size = new Size(800, 600);

            this.KeyDown += KeyDownHandler;

            pictureBox = new PictureBox();

            pictureBox.Bounds = this.Bounds;
            pictureBox.Location = new Point(0, 0);

            pictureBox.MouseDown += MouseDownHandler;
            pictureBox.MouseUp += MouseUpHandler;
            pictureBox.MouseMove += MouseMoveHandler;

            this.Controls.Add(pictureBox);

            pictureBox.BackColor = Color.Red;

            _canvas = new Bitmap(pictureBox.Width, pictureBox.Height);
            _graphics = Graphics.FromImage(_canvas);

            Setup();
        }

        Vector2 _gravity;
        List<Particle> _particles;
        List<Spring> _springs;

        private void Setup()
        {
            _gravity = new Vector2(0, .5f);

            _particles = new List<Particle>();
            _springs = new List<Spring>();

            _particles.Add(new Particle(400, 400));
            _particles.Add(new Particle(400, 50, true));
            _springs.Add(new Spring(.01f, 200, _particles[0], _particles[1]));

            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = 10;
            timer.Tick += Timer_Tick;
            timer.Start();

            new Thread(MouseControlHandler).Start();
        }

        Bitmap _canvas;
        Graphics _graphics;

        PointF _fakePoint;

        private void Timer_Tick(object sender, EventArgs e)
        {
            _graphics.Clear(Color.Black);

            if (_selectedRightParticle != null && _fakePoint != null)
            {
                _graphics.DrawLine(Pens.Red, _selectedRightParticle.Position.X, _selectedRightParticle.Position.Y, _fakePoint.X, _fakePoint.Y);
            }

            if (!_pauseSimulation)
            {
                foreach (Spring spring in _springs)
                {
                    spring.Update();
                }

                foreach (Particle particle in _particles)
                {
                    if (_applyGravity)
                        particle.ApplyForce(_gravity);
                    particle.Update();
                }
            }
            foreach (Spring spring in _springs)
            {
                DrawSpring(spring);

            }

            foreach (Particle particle in _particles)
            {
                DrawParticle(particle);

            }


            pictureBox.Image = _canvas;
        }

        private void DrawParticle(Particle particle, float radius = 25)
        {

            try
            {
                Brush brush = particle.Locked ? Brushes.MediumVioletRed: Brushes.White;
                _graphics.FillEllipse(brush, particle.Position.X - radius / 2, particle.Position.Y - radius / 2, 25, 25);
            }
            catch { }
        }

        private void DrawSpring(Spring spring)
        {
            try
            {
                Pen pen = Pens.White;
                _graphics.DrawLine(pen, spring.A.Position.X, spring.A.Position.Y, spring.B.Position.X, spring.B.Position.Y);
            }
            catch { }
        }

        private void KeyDownHandler(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Space:
                    _pauseSimulation = !_pauseSimulation;
                    break;
                case Keys.G:
                case Keys.Shift:
                    _applyGravity = !_applyGravity;
                    break;
                case Keys.R:
                    foreach (Particle particle in _particles)
                    {
                        particle.ResetMomentum();
                    }
                    break;
                case Keys.Escape:
                    Environment.Exit(0);
                    break;
            }
        }
        #region NEW_MOUSE_HANDLING
        private const float CLICK_DURATION = .3f * 1000;
        private const float PARTICLE_DETECTION_RANGE = 50f;
        Stopwatch _leftClickStopwatch = new Stopwatch();
        Stopwatch _rightClickStopwatch = new Stopwatch();
        Particle _selectedLeftParticle = null;
        Particle _selectedRightParticle = null;
        Particle _springSelectedParticle = null;
        private void MouseControlHandler()
        {
            for (; ; )
            {
                if (_leftClickStopwatch.ElapsedMilliseconds >= CLICK_DURATION && _selectedLeftParticle != null)
                {
                    _selectedLeftParticle.MoveToPoint(Cursor.Position);
                }

                if (_rightClickStopwatch.ElapsedMilliseconds >= CLICK_DURATION)
                {
                    _springSelectedParticle = GetCloseParticle(Cursor.Position.X, Cursor.Position.Y);
                    _fakePoint = _springSelectedParticle == null ? Cursor.Position : new PointF(_springSelectedParticle.Position.X, _springSelectedParticle.Position.Y);
                }
            }
        }
        private void MouseDownHandler(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    _selectedLeftParticle = GetCloseParticle(e.X, e.Y);
                    _leftClickStopwatch.Start();
                    break;
                case MouseButtons.Right:
                    _selectedRightParticle = GetCloseParticle(e.X, e.Y);
                    _rightClickStopwatch.Start();
                    break;
            }
        }

        private void MouseMoveHandler(object sender, MouseEventArgs e)
        {

        }

        private void MouseUpHandler(object sender, MouseEventArgs e)
        {

            if (_leftClickStopwatch.ElapsedMilliseconds < CLICK_DURATION && _leftClickStopwatch.IsRunning)
            {
                //Left click
                HandleLeftClick(e);
            }
            if (_rightClickStopwatch.IsRunning)
            {
                if (_rightClickStopwatch.ElapsedMilliseconds < CLICK_DURATION)
                {
                    //Right click
                    HandleRightClick(e);
                }
                else if (_selectedRightParticle != null && _springSelectedParticle != null)
                {
                    float length = (float)Math.Sqrt(Math.Pow(_selectedRightParticle.Position.X - _springSelectedParticle.Position.X, 2) + Math.Pow(_selectedRightParticle.Position.Y - _springSelectedParticle.Position.Y, 2));
                    _springs.Add(new Spring(DEFAULT_K, length, _selectedRightParticle, _springSelectedParticle));
                }
            }

            _selectedLeftParticle = null;
            _selectedRightParticle = null;
            _springSelectedParticle = null;

            switch (e.Button)
            {
                case MouseButtons.Left:
                    if (_leftClickStopwatch.IsRunning)
                    {
                        _leftClickStopwatch.Stop();
                        _leftClickStopwatch.Reset();
                    }
                    break;
                case MouseButtons.Right:
                    if (_rightClickStopwatch.IsRunning)
                    {
                        _rightClickStopwatch.Stop();
                        _rightClickStopwatch.Reset();
                    }
                    break;
                case MouseButtons.Middle:
                    HandleMiddleClick(e);
                    break;
            }
        }

        private void HandleLeftClick(MouseEventArgs e)
        {
            foreach (Particle particle in _particles)
            {
                if (CheckForParticle(particle, e.X, e.Y))
                {
                    RemoveSprings(particle);
                    _particles.Remove(particle);
                    return;
                }
            }
            _particles.Add(new Particle(e.X, e.Y));
        }

        private void HandleMiddleClick(MouseEventArgs e)
        {
            foreach (Particle particle in _particles)
            {
                if (CheckForParticle(particle, e.X, e.Y))
                {
                    particle.ResetMomentum();
                    particle.Locked = !particle.Locked;
                    return;
                }
            }
        }

        private void HandleRightClick(MouseEventArgs e)
        {
            foreach (Particle particle in _particles)
            {
                if (CheckForParticle(particle, e.X, e.Y))
                {
                    RemoveSprings(particle);
                    return;
                }
            }
        }

        private Particle GetCloseParticle(float x, float y)
        {
            foreach (Particle particle in _particles)
            {
                if (CheckForParticle(particle, x, y))
                    return particle;
            }
            return null;
        }

        private bool CheckForParticle(Particle particle, float x, float y)
        {
            float squaredXDiff = (float)Math.Pow(particle.Position.X - x, 2);
            float squaredYDiff = (float)Math.Pow(particle.Position.Y - y, 2);
            return squaredXDiff + squaredYDiff <= Math.Pow(PARTICLE_DETECTION_RANGE, 2);
        }

        private void RemoveSprings(Particle particle)
        {
            List<Spring> springsToRemove = new List<Spring>();
            foreach (Spring spring in _springs)
            {
                if (spring.A == particle || spring.B == particle)
                {
                    springsToRemove.Add(spring);
                }
            }

            foreach (Spring spring in springsToRemove)
            {
                _springs.Remove(spring);
            }
        }
        #endregion
        /*
        #region OLD_MOUSE_HANDLING
        
        private Particle _selectedParticle = null;
        private bool _selectedParticleLocked;
        private bool _rightMouseDown = false;
        private bool _leftMouseDown = false;
        private const float _particleDistanceTolerance = 50f;
        private void MouseDownHandler(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                _leftMouseDown = true;

            if (_leftMouseDown && _selectedParticle == null)
            {
                _leftMouseDown = true;


                foreach (Particle particle in _particles)
                {
                    if (Math.Pow(e.Location.X - particle.Position.X, 2) + Math.Pow(e.Location.Y - particle.Position.Y, 2) <= _particleDistanceTolerance)
                    {
                        _selectedParticle = particle;
                        _selectedParticleLocked = _selectedParticle.Locked;
                        _selectedParticle.Locked = true;
                        return;
                    }
                }
            }
        }


        private void MouseMoveHandler(object sender, MouseEventArgs e)
        {
            if (_selectedParticle != null && _leftMouseDown)
            {
                _selectedParticle.MoveToPoint(e.Location);
            }
        }

        private void MouseUpHandler(object sender, MouseEventArgs e)
        {
            if (_leftMouseDown && _selectedParticle != null && e.Button == MouseButtons.Left)
            {
                _leftMouseDown = false;
                _selectedParticle.Locked = _selectedParticleLocked;
                _selectedParticle = null;
            }
        }
        
        #endregion
        */
    }
}
