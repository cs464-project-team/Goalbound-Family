import ReceiptUploadWithAssignment from '../components/ReceiptUploadWithAssignment';

export default function ReceiptScanner() {
  return (
    <div style={{
      minHeight: '100vh',
      background: 'linear-gradient(to bottom right, #f8fafc 0%, #e0e7ff 100%)',
      padding: '2rem 0'
    }}>
      <div style={{ maxWidth: '1600px', margin: '0 auto', padding: '0 1rem' }}>
        <div style={{ marginBottom: '2rem', textAlign: 'center' }}>
          <h1 style={{
            fontSize: '2.75rem',
            fontWeight: '700',
            marginBottom: '0.75rem',
            background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
            WebkitBackgroundClip: 'text',
            WebkitTextFillColor: 'transparent',
            letterSpacing: '-0.5px'
          }}>
            Receipt Scanner
          </h1>
          <p style={{ color: '#64748b', fontSize: '1.05rem', fontWeight: '400' }}>
            Upload and process your receipts with AI-powered OCR
          </p>
        </div>
        <ReceiptUploadWithAssignment />
      </div>
    </div>
  );
}
