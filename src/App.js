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

  const [downloadUrl, setDownloadUrl] = useState(null);
  const [isPolling, setIsPolling] = useState(false);

  function openModal() {
    setIsOpen(true);
  }

  function closeModal() {
    setIsOpen(false);
  }

  const [textInput, setText] = useState("Detecting multiple leviathan class lifeforms in the region. Are you certain whatever you're doing is worth it?");

  const handleButtonClick = (setMessage) => {
  (async function () {
    if (textInput.trim() === "") {
      setMessage("Please enter text!");
      openModal();
      return;
    }
    try {
      setDownloadUrl(null);  // clear previous download
      setIsPolling(true);

      const input = {
        message: textInput,
        use_ssml: false,
        voice_id: "pda"
      };

      const request = { input };

      const response = await fetch("/api/generate", {
        method: "POST",
        headers: {
          "Content-Type": "application/json"
        },
        body: JSON.stringify(request)
      });

      const json = await response.json();
      const jobId = json.job_id;

      // Polling loop
      const pollInterval = 3000;
      let attempts = 0;
      const maxAttempts = 20;

      const pollStatus = async () => {
        const statusResponse = await fetch(`/api/status/${jobId}`);
        const statusJson = await statusResponse.json();

        if (statusJson.status === "ready") {
          setDownloadUrl(statusJson.url);
          setIsPolling(false);
        } else if (statusJson.status === "error") {
          setMessage("An internal server error occurred.");
          setIsPolling(false);
          openModal();
        }
        else if (attempts < maxAttempts) {
          attempts++;
          setTimeout(pollStatus, pollInterval);
        } else {
          setMessage("File not ready after waiting.");
          setIsPolling(false);
          openModal();
        }
      };

      pollStatus();

    } catch (error) {
      setMessage("An unknown error occurred.");
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
      {isPolling && <p>Waiting for file to be ready...</p>}
        {downloadUrl && (
          <div style={{
            marginLeft: '0px'
          }}>
            <h2>Your file is ready!</h2>
            <audio controls>
            <source src={downloadUrl} type="audio/wav" />
            Your browser does not support the audio element.
          </audio>
          <br/>
          <a href={downloadUrl} download target="_blank" rel="noopener noreferrer">
            Download Voice Line
          </a>
          </div>
        )}
    </section>
  )
}

function SiteFooter() {
  return (
    <footer>
      <p>This site is not officially affiliated with Krafton or Unknown Worlds.</p>
      <p>Uploaded text and generated files are stored temporarily. Generated files are deleted within seven days. The tool does not use artificial intelligence to generate output files.</p>
    </footer>
  )
}

export default App;
