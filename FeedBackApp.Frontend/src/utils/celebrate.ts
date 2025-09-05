export function celebrateConfettiRed() {
  const reduced = window.matchMedia?.("(prefers-reduced-motion: reduce)")?.matches;
  if (reduced) return;

  import("canvas-confetti").then(({ default: confetti }) => {
    confetti({
      particleCount: 80,
      spread: 65,
      origin: { y: 0.6 },
      colors: ["#dc2626", "#ef4444", "#b91c1c"],
      ticks: 180,
      gravity: 1.0,
    });

    const duration = 800;
    const end = Date.now() + duration;

    (function frame() {
      confetti({
        particleCount: 10,
        angle: 60,
        spread: 55,
        origin: { x: 0, y: 0.7 },
        colors: ["#dc2626"],
        ticks: 140,
        gravity: 1.1,
      });
      confetti({
        particleCount: 10,
        angle: 120,
        spread: 55,
        origin: { x: 1, y: 0.7 },
        colors: ["#b91c1c"],
        ticks: 140,
        gravity: 1.1,
      });

      if (Date.now() < end) requestAnimationFrame(frame);
    })();
  });
}
