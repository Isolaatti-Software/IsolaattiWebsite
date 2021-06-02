let markdownContainers = document.querySelectorAll(".markdown");
for (let container of markdownContainers) {
    container.innerHTML = marked(container.innerText);
}