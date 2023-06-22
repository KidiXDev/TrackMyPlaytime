var slideIndex = 0;
var screenshotSlider = document.querySelector(".screenshot-slider");
var screenshots = document.querySelectorAll(".screenshot");
var sliderControls = document.querySelectorAll(".slider-control");
var timeout;

function slide(direction) {
  if (direction === "left") {
    slideIndex--;
    if (slideIndex < 0) {
      slideIndex = screenshots.length - 1;
    }
  } else if (direction === "right") {
    slideIndex++;
    if (slideIndex >= screenshots.length) {
      slideIndex = 0;
    }
  }

  screenshotSlider.style.transform = "translateX(-" + slideIndex * 100 + "%)";

  clearTimeout(timeout);
  timeout = setTimeout(autoSlide, 5000);
}

function autoSlide() {
  slide("right");

  timeout = setTimeout(autoSlide, 5000);
}

autoSlide();
