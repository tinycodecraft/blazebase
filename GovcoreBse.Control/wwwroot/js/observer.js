export function initObserver(component, observerTargetId,observerContainerId) {

    let element = document.getElementById(observerTargetId);
    let container = document.getElementById(observerContainerId);

    var observer = new IntersectionObserver(e => {
        console.log(JSON.stringify(e));
        component.invokeMethodAsync('OnIntersection');
    }, { root: container, rootMargin: '0px', threshold: 0.5 });

    if (element == null) throw new Error("The observable target was not found");
    observer.observe(element);
    window.Observer = observer;
    return observer;

}

