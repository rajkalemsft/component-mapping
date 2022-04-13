using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace component_mapper
{
    public class DependencyManager
    {
        /// <summary>
        /// Key-ComponentName, Value - List<DependentComponentNames>
        /// </summary>
        private Dictionary<string, List<string>> componentDependencies = new Dictionary<string, List<string>>();

        /// <summary>
        /// Key-ComponentName, Value- DepdendencyCount
        /// </summary>
        private Dictionary<string, int> installedComponents = new Dictionary<string, int>();

        public void AddDependency(string componentName, List<string> dependentComponent)
        {
            if(string.IsNullOrEmpty(componentName) || dependentComponent == null)
            {
                Console.WriteLine($"Invalid dependency definition. Ignoring Command. ComponentName :{componentName}");
                return;
            }

            //Avoid duplicate dependency addition
            if (componentDependencies.ContainsKey(componentName))
            {
                Console.WriteLine($"Dependency already defined. Ignoring Command. ComponentName :{componentName}, Dependencies: {string.Join(" ", dependentComponent)}");
                return;
            }
            var circularDependency = dependentComponent.Any(d => 
                                    componentDependencies.ContainsKey(d) //If new component's dependent component already has the dependency defined
                                    && componentDependencies[d].Any(c => c.Equals(componentName, StringComparison.OrdinalIgnoreCase))); //and if the new component's dependent component has dependency on new component to be added

            if (circularDependency)
            {
                Console.WriteLine($"Circular dependency. Ignoring Command. ComponentName :{componentName}, Dependencies: {string.Join(" ", dependentComponent)}");
                return;
            }

            //Add to dependency table
            componentDependencies.Add(componentName, dependentComponent);
        }

        public void InstallComponent(string componentName)
        {
            //1. Resolve the dependency tree and get list of all the components required
            var componentsToBeInstalledInOrder = ResolveDependencyTree(componentName);

            //2. Install in FIFO order. Skip already installed

            while (componentsToBeInstalledInOrder.Count() > 0)
            {
                var component = componentsToBeInstalledInOrder.Dequeue();

                if (installedComponents.ContainsKey(component))
                {
                    Console.WriteLine($"{component} is already Installed.");
                    installedComponents[component] += 1;
                    return;
                }
                Console.WriteLine($"Installing :{componentName}");
                installedComponents.Add(component, 1);


                //This is to make sure our ComponentDependencies data is source of truth for all components
                AddDependency(component, new List<string>());
            }

        }

        private Queue<string> ResolveDependencyTree(string componentName, Queue<string> componentsToBeInstalledInOrder = null)
        {
            if (componentsToBeInstalledInOrder == null)
            {
                componentsToBeInstalledInOrder = new Queue<string>();
            }

            //Component has dependencies
            if (componentDependencies.ContainsKey(componentName) && componentDependencies[componentName].Count() > 0)
            {
                foreach (var dependency in componentDependencies[componentName])
                {
                    ResolveDependencyTree(dependency, componentsToBeInstalledInOrder);
                }
            }

            if (!componentsToBeInstalledInOrder.Contains(componentName))
            {
                componentsToBeInstalledInOrder.Enqueue(componentName);
            }

            return componentsToBeInstalledInOrder;
        }

        public void RemoveComponent(string componentName)
        {
            if (string.IsNullOrEmpty(componentName))
            {
                Console.WriteLine($"Invalid component. Ignoring Command.");
                return;
            }

            if (installedComponents.Count() == 0  || !installedComponents.ContainsKey(componentName))
            {
                Console.WriteLine($"{componentName} Not Installed.");
                return;
            }

            if (installedComponents.ContainsKey(componentName) && installedComponents[componentName] > 1)
            {
                Console.WriteLine($"{componentName} is still needed.");
                return;
            }

            //Reduce the dependency count
            if(componentDependencies.ContainsKey(componentName) && componentDependencies.Count() > 1)
            {
                foreach (var item in componentDependencies[componentName])
                {
                    installedComponents[item] -= 1;
                }
            }
            installedComponents.Remove(componentName);
            Console.WriteLine($"Removing {componentName}.");
        }

        public void ListInstalled()
        {
            foreach (var item in installedComponents)
            {
                Console.WriteLine($"{item}");
            }
        }
    }
}
