export default function Impressum() {
  return (
    <div className="max-w-4xl mx-auto px-4 py-12">
      <h1 className="text-3xl font-bold mb-8">Impressum</h1>
      
      <div className="space-y-6">
        <section>
          <h2 className="text-xl font-semibold mb-3">Angaben gemäß § 5 DDG</h2>
          <p>power4you GmbH<br />
          Musterstraße 123<br />
          12345 Stadt</p>
        </section>

        <section>
          <h2 className="text-xl font-semibold mb-3">Kontakt</h2>
          <p>Telefon: +49 (0) 123 456789<br />
          E-Mail: info@power4you.de</p>
        </section>

        <section>
          <h2 className="text-xl font-semibold mb-3">Vertreten durch</h2>
          <p>Geschäftsführer: Max Mustermann</p>
        </section>

        <section>
          <h2 className="text-xl font-semibold mb-3">Registereintrag</h2>
          <p>Eintragung im Handelsregister.<br />
          Registergericht: Amtsgericht Stadt<br />
          Registernummer: HRB 12345</p>
        </section>

        <section>
          <h2 className="text-xl font-semibold mb-3">Umsatzsteuer-ID</h2>
          <p>Umsatzsteuer-Identifikationsnummer gemäß §27 a Umsatzsteuergesetz:<br />
          DE 123456789</p>
        </section>

        <section>
          <h2 className="text-xl font-semibold mb-3">Verantwortlich für den Inhalt nach § 55 Abs. 2 RStV</h2>
          <p>Max Mustermann<br />
          Musterstraße 123<br />
          12345 Stadt</p>
        </section>
      </div>
    </div>
  );
} 