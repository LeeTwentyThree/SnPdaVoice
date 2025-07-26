import React, { useState } from 'react';
import ReactModal from 'react-modal';
import Modal from 'react-modal';

Modal.setAppElement('#root');

function App() {
  return (
    <>
      <div id="overlays" />
      <div id="page-content">
        <SiteHeader />
        <main>
          <GenerateMainSection />
        </main>
      </div>
      <SiteFooter />
    </>
  );
}

function SiteHeader() {
  return (
    <div>
      <h1>Subnautica PDA Voice Generator</h1>
      <hr />
    </div>
  );
}

function GenerateMainSection() {
  const [modalErrorMessage, setErrorMessage] = React.useState("An error occurred");
  const [modalIsOpen, setIsOpen] = React.useState(false);

  function openModal() {
    setIsOpen(true);
  }

  function closeModal() {
    setIsOpen(false);
  }

  const [textInput, setText] = useState("Detecting multiple leviathan class lifeforms in the region. Are you certain whatever you're doing is worth it?");

  const handleButtonClick = (setMessage) => {
    (async function () {
      try {
        const response = await fetch(`/api/generate`);
        const full = await response.text();
        setMessage("Full response: " + full);
        openModal();
        // const data = await response.json();
        // alert("Result:\n" + data.message);
      }
      catch (error) {
        if (error.response) {
          setMessage("Error accessing backend: " + error.response.status);
        }
        else {
          setMessage("An unknown error occurred");
        }
        openModal();
      }
    })();
  };

  return (
    <section>
      <p>Insert text for voice line generation:</p>
      <textarea
        value={textInput}
        onChange={(e) => setText(e.target.value)}
        style={{
          width: '80%',
          height: '150px',         // default height
          resize: 'vertical',      // allow user to resize up/down only
          padding: '8px',
          fontSize: '1rem',
        }}
      />
      <div class='button-spacing'></div>
      <button
        type="button"
        onClick={() => {
          handleButtonClick(setErrorMessage);
        }}
        id='generate-button'>
        Generate
      </button>
      <ReactModal
        isOpen={modalIsOpen}
        onRequestClose={closeModal}
        contentLabel="Error while contacting API"
        className="Modal"
        overlayClassName="Overlay"
      >
        <div style={{
          gap: '10px'
        }}>
          <h2>{modalErrorMessage}</h2>
          <button onClick={closeModal}>Close</button>
        </div>
      </ReactModal>
    </section>
  )
}

function SiteFooter() {
  return (
    <footer>
      <p>This site is not officially affiliated with Krafton or Unknown Worlds.</p>
      <p>The tool does not use artificial intelligence to generate output files.</p>
    </footer>
  )
}

export default App;
