using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Tetris
{
    public partial class Form1 : Form
    {
        const int Columnas = 10;
        const int Filas = 20;
        const int TamCelda = 25;

        int[,] tablero = new int[Filas, Columnas];

        static readonly int[][,] Piezas = new int[][,]
        {
            new int[,] { {0,0,0,0}, {1,1,1,1}, {0,0,0,0}, {0,0,0,0} }, // I
            new int[,] { {0,1,1,0}, {0,1,1,0}, {0,0,0,0}, {0,0,0,0} }, // O
            new int[,] { {0,1,0,0}, {1,1,1,0}, {0,0,0,0}, {0,0,0,0} }, // T
            new int[,] { {0,1,1,0}, {1,1,0,0}, {0,0,0,0}, {0,0,0,0} }, // S
            new int[,] { {1,1,0,0}, {0,1,1,0}, {0,0,0,0}, {0,0,0,0} }, // Z
            new int[,] { {1,0,0,0}, {1,1,1,0}, {0,0,0,0}, {0,0,0,0} }, // J
            new int[,] { {0,0,1,0}, {1,1,1,0}, {0,0,0,0}, {0,0,0,0} }, // L
        };

        static readonly Color[] Colores = new Color[]
        {
            Color.Cyan, Color.Yellow, Color.Purple, Color.Green,
            Color.Red, Color.Blue, Color.Orange
        };

        int[,] piezaActual;
        int colorActual;
        int piezaFila, piezaCol;

        Random random = new Random();
        System.Windows.Forms.Timer timer;
        int puntaje = 0;
        bool juegoTerminado = false;

        Label lblPuntaje;

        public Form1()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.ClientSize = new Size(Columnas * TamCelda + 150, Filas * TamCelda);
            this.Text = "Tetris";
            this.KeyPreview = true;

            lblPuntaje = new Label();
            lblPuntaje.Location = new Point(Columnas * TamCelda + 20, 20);
            lblPuntaje.AutoSize = true;
            lblPuntaje.Font = new Font("Consolas", 12);
            this.Controls.Add(lblPuntaje);

            this.KeyDown += Form1_KeyDown;
            this.Paint += Form1_Paint;

            timer = new System.Windows.Forms.Timer();
            timer.Interval = 500;
            timer.Tick += Timer_Tick;

            NuevaPieza();
            timer.Start();
            ActualizarPuntaje();
        }

        void NuevaPieza()
        {
            int indice = random.Next(Piezas.Length);
            piezaActual = (int[,])Piezas[indice].Clone();
            colorActual = indice + 1;
            piezaFila = 0;
            piezaCol = Columnas / 2 - 2;

            if (ColisionA(piezaFila, piezaCol, piezaActual))
            {
                juegoTerminado = true;
                timer.Stop();

                DialogResult respuesta = MessageBox.Show(
                    $"Juego terminado. Puntaje final: {puntaje}\n\n¿Quieres jugar otra vez?",
                    "Fin del juego",
                    MessageBoxButtons.YesNo);

                if (respuesta == DialogResult.Yes)
                {
                    ReiniciarJuego();
                }
                else
                {
                    Application.Exit();
                }
            }
        }

        void ReiniciarJuego()
        {
            // Limpia el tablero
            for (int f = 0; f < Filas; f++)
                for (int c = 0; c < Columnas; c++)
                    tablero[f, c] = 0;

            puntaje = 0;
            timer.Interval = 500;
            juegoTerminado = false;
            ActualizarPuntaje();

            NuevaPieza();
            timer.Start();
            Invalidate();
        }

        bool ColisionA(int filaBase, int colBase, int[,] forma)
        {
            for (int f = 0; f < 4; f++)
            {
                for (int c = 0; c < 4; c++)
                {
                    if (forma[f, c] == 0) continue;

                    int fila = filaBase + f;
                    int col = colBase + c;

                    if (col < 0 || col >= Columnas || fila >= Filas)
                        return true;

                    if (fila >= 0 && tablero[fila, col] != 0)
                        return true;
                }
            }
            return false;
        }

        int[,] Rotar(int[,] forma)
        {
            int[,] rotada = new int[4, 4];
            for (int f = 0; f < 4; f++)
            {
                for (int c = 0; c < 4; c++)
                {
                    rotada[c, 3 - f] = forma[f, c];
                }
            }
            return rotada;
        }

        void Timer_Tick(object sender, EventArgs e)
        {
            MoverAbajo();
        }

        void MoverAbajo()
        {
            if (juegoTerminado) return;

            if (!ColisionA(piezaFila + 1, piezaCol, piezaActual))
            {
                piezaFila++;
            }
            else
            {
                FijarPieza();
                LimpiarLineas();
                NuevaPieza();
            }

            Invalidate();
        }

        void FijarPieza()
        {
            for (int f = 0; f < 4; f++)
            {
                for (int c = 0; c < 4; c++)
                {
                    if (piezaActual[f, c] == 0) continue;

                    int fila = piezaFila + f;
                    int col = piezaCol + c;

                    if (fila >= 0 && fila < Filas && col >= 0 && col < Columnas)
                        tablero[fila, col] = colorActual;
                }
            }
        }

        void LimpiarLineas()
        {
            int lineasEliminadas = 0;

            for (int fila = Filas - 1; fila >= 0; fila--)
            {
                bool completa = true;
                for (int col = 0; col < Columnas; col++)
                {
                    if (tablero[fila, col] == 0)
                    {
                        completa = false;
                        break;
                    }
                }

                if (completa)
                {
                    for (int f = fila; f > 0; f--)
                    {
                        for (int c = 0; c < Columnas; c++)
                        {
                            tablero[f, c] = tablero[f - 1, c];
                        }
                    }
                    for (int c = 0; c < Columnas; c++)
                        tablero[0, c] = 0;

                    lineasEliminadas++;
                    fila++;
                }
            }

            if (lineasEliminadas > 0)
            {
                puntaje += lineasEliminadas * 100;
                ActualizarPuntaje();

                if (timer.Interval > 100)
                    timer.Interval -= 10;
            }
        }

        void ActualizarPuntaje()
        {
            lblPuntaje.Text = $"Puntaje:\n{puntaje}";
        }

        void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (juegoTerminado) return;

            switch (e.KeyCode)
            {
                case Keys.Left:
                    if (!ColisionA(piezaFila, piezaCol - 1, piezaActual))
                        piezaCol--;
                    break;

                case Keys.Right:
                    if (!ColisionA(piezaFila, piezaCol + 1, piezaActual))
                        piezaCol++;
                    break;

                case Keys.Down:
                    MoverAbajo();
                    break;

                case Keys.Up:
                    int[,] rotada = Rotar(piezaActual);
                    if (!ColisionA(piezaFila, piezaCol, rotada))
                        piezaActual = rotada;
                    break;

                case Keys.Space:
                    while (!ColisionA(piezaFila + 1, piezaCol, piezaActual))
                        piezaFila++;
                    MoverAbajo();
                    break;
            }

            Invalidate();
        }

        void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            for (int fila = 0; fila < Filas; fila++)
            {
                for (int col = 0; col < Columnas; col++)
                {
                    if (tablero[fila, col] != 0)
                    {
                        DibujarCelda(g, fila, col, Colores[tablero[fila, col] - 1]);
                    }
                    else
                    {
                        g.DrawRectangle(Pens.Gainsboro, col * TamCelda, fila * TamCelda, TamCelda, TamCelda);
                    }
                }
            }

            for (int f = 0; f < 4; f++)
            {
                for (int c = 0; c < 4; c++)
                {
                    if (piezaActual[f, c] != 0)
                    {
                        DibujarCelda(g, piezaFila + f, piezaCol + c, Colores[colorActual - 1]);
                    }
                }
            }

            g.DrawRectangle(Pens.Black, 0, 0, Columnas * TamCelda, Filas * TamCelda);
        }

        void DibujarCelda(Graphics g, int fila, int col, Color color)
        {
            if (fila < 0) return;

            Rectangle rect = new Rectangle(col * TamCelda, fila * TamCelda, TamCelda, TamCelda);
            g.FillRectangle(new SolidBrush(color), rect);
            g.DrawRectangle(Pens.Black, rect);
        }
    }
}