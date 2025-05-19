import { useState } from "react";
import { motion } from "framer-motion";
import LoginForm from "./LoginForm";
import RegisterForm from "./RegisterForm";

export default function AuthCard() {
  const gradients = {
    login: "linear-gradient(to bottom right, rgb(3, 218, 168), rgb(87, 59, 246), rgb(136, 86, 255))",
    register: "linear-gradient(to bottom right, rgb(136, 86, 255), rgb(87, 59, 246), rgb(3, 218, 168))",
  };

  const [isRegister, setIsRegister] = useState(false);
  const [rotation, setRotation] = useState(0);
  const [bgGradient, setBgGradient] = useState(gradients.login);

  const handleSwitch = () => {
    setRotation(prev => prev + 180);
    setBgGradient(isRegister ? gradients.login : gradients.register);
    setTimeout(() => setIsRegister(prev => !prev), 750);
  };

  return (
    <motion.div
      style={{ backgroundImage: bgGradient }}
      animate={{ backgroundImage: bgGradient }}
      transition={{ duration: 1, ease: [0.4, 0.0, 0.2, 1] }}
      className="flex items-center justify-center h-screen overflow-hidden bg-cover bg-no-repeat"
    >
      <div
        style={{ perspective: 1000 }}
        className="w-[400px] h-[400px] flex items-center justify-center"
      >
        <motion.div
          animate={{ rotateY: rotation }}
          transition={{ duration: 1.2, ease: [0.4, 0.0, 0.2, 1] }}
          style={{
            width: "100%",
            height: "100%",
            transformStyle: "preserve-3d",
            position: "relative",
          }}
        >
          {/* Login Side */}
          <div
            style={{
              backfaceVisibility: "hidden",
              position: "absolute",
              width: "100%",
              height: "100%",
              top: 0,
              left: 0,
            }}
          >
            <LoginForm onSwitch={handleSwitch} />
          </div>

          {/* Register Side */}
          <div
            style={{
              backfaceVisibility: "hidden",
              position: "absolute",
              width: "100%",
              height: "100%",
              top: 0,
              left: 0,
              transform: "rotateY(180deg)",
            }}
          >
            <RegisterForm onSwitch={handleSwitch} />
          </div>
        </motion.div>
      </div>
    </motion.div>
  );
}
