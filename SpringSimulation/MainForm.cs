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

            canvas = new Bitmap(pictureBox.Width, pictureBox.Height);
            graphics = Graphics.FromImage(canvas);

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

        Bitmap canvas;
        Graphics graphics;

        private void Timer_Tick(object sender, EventArgs e)
        {
            graphics.Clear(Color.Black);

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


            pictureBox.Image = canvas;
        }

        private void DrawParticle(Particle particle, float radius = 25)
        {

            graphics.FillEllipse(Brushes.White, particle.Position.X - radius / 2, particle.Position.Y - radius / 2, 25, 25);
        }

        private void DrawSpring(Spring spring)
        {
            graphics.DrawLine(Pens.White, spring.A.Position.X, spring.A.Position.Y, spring.B.Position.X, spring.B.Position.Y);
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
            }
        }

        private const float CLICK_DURATION = .3f * 1000;
        Stopwatch _leftClickStopwatch = new Stopwatch();
        Stopwatch _rightClickStopwatch = new Stopwatch();
        Particle _selectedParticle = null;
        private void MouseControlHandler()
        {
            for (; ; )
            {
                if (_leftClickStopwatch.ElapsedMilliseconds >= CLICK_DURATION)
                {
                    //Left drag
                }

                if (_rightClickStopwatch.ElapsedMilliseconds >= CLICK_DURATION)
                {
                    //Right drag
                }
            }
        }
        private void MouseDownHandler(object sender, MouseEventArgs e)
        {
            switch(e.Button)
            {
                case MouseButtons.Left:
                    _leftClickStopwatch.Start();
                    break;
                case MouseButtons.Right:
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
            }

            if(_rightClickStopwatch.ElapsedMilliseconds < CLICK_DURATION && _rightClickStopwatch.IsRunning)
            {
                //Right click
            }

            switch (e.Button)
            {
                case MouseButtons.Left:
                    if (_leftClickStopwatch.IsRunning)
                        _leftClickStopwatch.Stop();
                    break;
                case MouseButtons.Right:
                    if (_rightClickStopwatch.IsRunning)
                        _rightClickStopwatch.Stop();
                    break;
            }
        }


        #region OLD_MOUSE_HANDLING
        /*
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
        */
        #endregion
    }
}
