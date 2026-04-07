using System;
using System.Windows.Forms;

namespace GravadorCensura
{
    static class Program
    {
        /// <summary>
        /// Ponto de entrada principal para o aplicativo.
        /// </summary>
        [STAThread] // Garante que a interface rode de forma correta no Windows
        static void Main()
        {
            // Ativa os estilos visuais modernos (bordas arredondadas, cores do Windows)
            Application.EnableVisualStyles();

            // Melhora a renderização de textos
            Application.SetCompatibleTextRenderingDefault(false);

            // Inicia o formulário principal
            // É aqui que o sistema "trava" e fica rodando até você fechar a janela
            Application.Run(new Form1());
        }
    }
}