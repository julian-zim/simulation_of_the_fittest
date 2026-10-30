# Simulation of the fittest

Simulated natural selection within a 3D Unity Engine environment.

Offspring gene logic:
float <gene>Mutation = ((UnityEngine.Random.value * 2 - 1) * <gene>MutationRange);
float offspringGenes.<gene> = (parent1Genes.<gene> + parent2Genes.<gene>) / 2 + <gene>Mutation;

https://github.com/user-attachments/assets/4ab9b9fa-174f-4d8c-9270-bc4bd992c8ce
