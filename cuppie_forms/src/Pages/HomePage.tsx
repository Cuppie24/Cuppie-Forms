export default function Home(){
    return(
    <div className="w-full min-h-screen bg-gradient-to-br from-[#1e1e1e] to-[#121212] text-white flex flex-col justify-center items-center p-4">
        <h1 className="text-4xl md:text-6xl font-bold text-center text-[#FF6B6B] drop-shadow-lg mb-4">
            Добро пожаловать
        </h1>
        <p className="text-lg md:text-xl text-gray-300 text-center max-w-xl mb-8">
            Это стартовая страница вашего нового проекта на React + Tailwind CSS. Наслаждайтесь разработкой!
        </p>
        <a
            href="#"
            className="px-6 py-3 bg-[#FF6B6B] hover:bg-[#ff4e4e] text-white rounded-xl shadow-lg transition-all duration-300 font-medium"
        >
            Начать
        </a>
    </div>
    );
}
