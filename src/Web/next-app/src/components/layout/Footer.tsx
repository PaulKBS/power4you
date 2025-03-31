import Link from 'next/link';

export default function Footer() {
  return (
    <footer className="bg-gray-900 text-white">
      <div className="max-w-7xl mx-auto px-4 py-8">
        <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
          <div className="space-y-3">
            <h3 className="text-base font-bold text-orange-500">power4you</h3>
            <p className="text-sm text-gray-400">Innovative Energielösungen für eine nachhaltige Zukunft</p>
          </div>
          <div>
            <h3 className="text-base font-semibold mb-2">Quick Links</h3>
            <ul className="space-y-1 text-sm">
              <li><Link href="#" className="text-gray-400 hover:text-white">Über uns</Link></li>
              <li><Link href="#" className="text-gray-400 hover:text-white">Karriere</Link></li>
              <li><Link href="#" className="text-gray-400 hover:text-white">FAQ</Link></li>
            </ul>
          </div>
          <div>
            <h3 className="text-base font-semibold mb-2">Rechtliches</h3>
            <ul className="space-y-1 text-sm">
              <li><Link href="#" className="text-gray-400 hover:text-white">Impressum</Link></li>
              <li><Link href="#" className="text-gray-400 hover:text-white">Datenschutz</Link></li>
              <li><Link href="#" className="text-gray-400 hover:text-white">AGB</Link></li>
            </ul>
          </div>
          <div>
            <h3 className="text-base font-semibold mb-2">Kontakt</h3>
            <ul className="space-y-1 text-sm text-gray-400">
              <li className="flex items-center space-x-2">
                <i className="fas fa-map-marker-alt"></i>
                <span>Musterstraße 123, 12345 Stadt</span>
              </li>
              <li className="flex items-center space-x-2">
                <i className="fas fa-envelope"></i>
                <span>info@power4you.de</span>
              </li>
            </ul>
          </div>
        </div>
        <div className="border-t border-gray-800 mt-6 pt-6 text-center text-sm text-gray-400">
          <p>&copy; 2024 power4you. Alle Rechte vorbehalten.</p>
        </div>
      </div>
    </footer>
  );
} 