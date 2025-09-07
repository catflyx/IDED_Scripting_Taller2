using System;
using System.Collections.Generic;
using System.Threading;


namespace TallerPoo
{
    // -------------------------------
    // Clase base: Nodo
    // -------------------------------
    abstract class Node
    {
        protected List<Node> children = new List<Node>();
        public abstract bool Execute();

        public virtual void AddChild(Node child)
        {
            children.Add(child);
        }
    }

    // -------------------------------
    // Nodo Raíz (1 hijo)
    // -------------------------------
    class Root : Node
    {
        public Root(Node child)
        {
            children.Add(child);
        }

        public override bool Execute()
        {
            return children[0].Execute();
        }
    }

    // -------------------------------
    // Nodo Compuesto (base)
    // -------------------------------
    abstract class Composite : Node
    {
    }

    // -------------------------------
    // Nodo Secuencia
    // -------------------------------
    class Sequence : Composite
    {
        public override bool Execute()
        {
            foreach (var child in children)
            {
                if (!child.Execute()) return false;
            }
            return true;
        }
    }

    // -------------------------------
    // Nodo Selector
    // -------------------------------
    class Selector : Composite
    {
        private Func<bool> condition;

        public Selector(Func<bool> condition = null)
        {
            this.condition = condition;
        }

        public override bool Execute()
        {
            if (condition != null && !condition())
                return false;

            foreach (var child in children)
            {
                if (child.Execute()) return true;
            }
            return false;
        }

        public bool Check()
        {
            return condition == null || condition();
        }
    }

    // -------------------------------
    // Nodo Tarea (hojas)
    // -------------------------------
    abstract class Task : Node
    {
        public override void AddChild(Node child)
        {
            throw new InvalidOperationException("Una tarea no puede tener hijos");
        }
    }

    // Tarea de moverse hacia el objetivo
    class MoveTask : Task
    {
        private int target;
        private Func<int> getPosition;
        private Action<int> setPosition;

        public MoveTask(Func<int> getPosition, Action<int> setPosition, int target)
        {
            this.getPosition = getPosition;
            this.setPosition = setPosition;
            this.target = target;
        }

        public override bool Execute()
        {
            Console.WriteLine("Iniciando movimiento hacia el objetivo...");
            while (getPosition() < target)
            {
                int nuevaPos = getPosition() + 1;
                setPosition(nuevaPos);
                Console.WriteLine($"Posición actual: {nuevaPos}");
                Thread.Sleep(500);
            }
            Console.WriteLine("Objetivo alcanzado!");
            return true;
        }
    }

    // Tarea de esperar
    class WaitTask : Task
    {
        private int waitMs;

        public WaitTask(int waitMs)
        {
            this.waitMs = waitMs;
        }

        public override bool Execute()
        {
            Console.WriteLine($"Esperando {waitMs / 1000} segundos...");
            Thread.Sleep(waitMs);
            return true;
        }
    }

    // -------------------------------
    // Programa principal
    // -------------------------------
    class Program
    {
        static void Main(string[] args)
        {
            int posicion = 0;
            int objetivo = 5;
            int distanciaValida = 10;
            int tiempoEspera = 2000;

            // Selector con condición de distancia
            Selector selector = new Selector(() =>
            {
                int distancia = objetivo - posicion;
                return distancia <= distanciaValida;
            });

            // Tarea de movimiento con acceso por referencia a la posición
            selector.AddChild(new MoveTask(
                () => posicion,          // getter
                x => posicion = x,       // setter
                objetivo
            ));

            // Secuencia: selector + esperar
            Sequence secuencia = new Sequence();
            secuencia.AddChild(selector);
            secuencia.AddChild(new WaitTask(tiempoEspera));

            // Raíz del árbol
            Root raiz = new Root(secuencia);

            // Simulación de la AI en bucle
            while (true)
            {
                raiz.Execute();
            }
        }
    }

}
