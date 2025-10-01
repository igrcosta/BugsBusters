//esse script separado só serve para armazenar uma lista de cores que vamos utilizar
//durante o código, por isso ele não é monobehavior. Aqui não vou usar os métodos
//que utilizamos na Unity, esse script só vai servir pra armazenar as cores que teremos no jogo

using UnityEngine;

public enum AffinityColorENUM
{
    X, //cor número 1
    Y //cor número 2
}

// de forma mais técnica, é como se tivéssemos criado um array global de cores que qualquer objeto
//pode usar, isso não muda visualmente os objetos, na verdade, a ideia disso aqui é que possamos
//deixar o mais modular possível. Então, a cor de cada gameobject é definida via código, o que vai
//facilitar nossa programação para a mecânica principal do jogo
